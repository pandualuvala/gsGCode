﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using g3;

namespace gs 
{
	// useful documents:
	//   https://github.com/makerbot/s3g/blob/master/doc/GCodeProtocol.md


	/// <summary>
	/// Makerbot GCode interpreter.
	/// </summary>
	public class MakerbotInterpreter : IGCodeInterpreter
	{
		IGCodeListener listener = null;

		Dictionary<int, Action<GCodeLine>> GCodeMap = new Dictionary<int, Action<GCodeLine>>();

		Vector3d CurPosition = Vector3d.Zero;

		double ExtrusionA = 0;
		double LastRetractA = 0;
		bool in_retract = false;

		public MakerbotInterpreter() {
			build_maps();			
		}

		public virtual void AddListener(IGCodeListener listener) 
		{
			if (this.listener != null)
				throw new Exception("Only one listener supported!");
			this.listener = listener;
		}


		public virtual void Interpret(GCodeFile file, InterpretArgs args)
		{
			IEnumerable<GCodeLine> lines_enum =
				(args.HasTypeFilter) ? file.AllLines() : file.AllLinesOfType(args.eTypeFilter);

			listener.Begin();

			ExtrusionA = 0;
			CurPosition = Vector3d.Zero;

			foreach(GCodeLine line in lines_enum) {

				if ( line.type == GCodeLine.LType.GCode ) {
					Action<GCodeLine> parseF;
					if (GCodeMap.TryGetValue(line.code, out parseF))
						parseF(line);
				}
			}

			listener.End();
		}



		void emit_linear(GCodeLine line)
		{
			Debug.Assert(line.code == 1);

			double x = GCodeUtil.UnspecifiedValue, 
				y = GCodeUtil.UnspecifiedValue, 
				z = GCodeUtil.UnspecifiedValue;
			bool absx = GCodeUtil.TryFindParamNum(line.parameters, "X", ref x);
			bool absy = GCodeUtil.TryFindParamNum(line.parameters, "Y", ref y);
			bool absz = GCodeUtil.TryFindParamNum(line.parameters, "Z", ref z);
			Vector3d newPos = CurPosition;
			if ( absx )
				newPos.x = x;
			if ( absy )
				newPos.y = y;
			if ( absz )
				newPos.z = z;
			CurPosition = newPos;

			// F is feed rate (this changes?)
			double f = 0;
			bool haveF = GCodeUtil.TryFindParamNum(line.parameters, "F", ref f);

			// A is extrusion stepper. E is also "current" stepper.
			double a = 0;
			bool haveA = GCodeUtil.TryFindParamNum(line.parameters, "A", ref a);
			if ( haveA == false ) {
				haveA = GCodeUtil.TryFindParamNum(line.parameters, "E", ref a);
			}

			LinearMoveData move = new LinearMoveData(
				newPos,
				(haveF) ? f : GCodeUtil.UnspecifiedValue,
				(haveA) ? GCodeUtil.Extrude(a) : GCodeUtil.UnspecifiedPosition );

			if ( haveA == false ) {
				// just ignore this state? happens a few times at startup...
				//Debug.Assert(in_retract);
			} else if (in_retract) {
				Debug.Assert(a <= LastRetractA);
				if ( MathUtil.EpsilonEqual(a, LastRetractA, 0.00001) ) {
					in_retract = false;
					listener.BeginDeposition();
					ExtrusionA = a;
				}
			} else if ( a < ExtrusionA ) {
				in_retract = true;
				LastRetractA = ExtrusionA;
				ExtrusionA = a;
				listener.BeginTravel();
			} else {
				ExtrusionA = a;
			}

			move.source = line;
			listener.LinearMoveToAbsolute3d(move);
		}



		// G92 - Position register: Set the specified axes positions to the given position
		// Sets the position of the state machine and the bot. NB: There are two methods of forming the G92 command:
		void set_position(GCodeLine line)
		{
			double x = 0, y = 0, z = 0, a = 0;
			if ( GCodeUtil.TryFindParamNum(line.parameters, "X", ref x ) ) {
				CurPosition.x = x;
			}
			if ( GCodeUtil.TryFindParamNum(line.parameters, "Y", ref y ) ) {
				CurPosition.y = y;
			}
			if ( GCodeUtil.TryFindParamNum(line.parameters, "Z", ref z ) ) {
				CurPosition.z = z;
			}
			if ( GCodeUtil.TryFindParamNum(line.parameters, "A", ref a ) ) {
				ExtrusionA = a;
				listener.CustomCommand(
					(int)CustomListenerCommands.ResetExtruder, GCodeUtil.Extrude(a) );
			}

			// E is "current" stepper (A for single extruder)
			double e = 0;
			if ( GCodeUtil.TryFindParamNum(line.parameters, "A", ref e ) ) {
				ExtrusionA = e;
				listener.CustomCommand(
					(int)CustomListenerCommands.ResetExtruder, GCodeUtil.Extrude(e) );
			}
		}



		void build_maps()
		{

			// G1 = linear move
			GCodeMap[1] = emit_linear;

			// G4 = CCW circular
			//GCodeMap[4] = emit_ccw_arc;
			//GCodeMap[5] = emit_cw_arc;

			GCodeMap[92] = set_position;
		}


	}
}