﻿﻿using System;
using g3;

namespace gs
{
    public enum MachineClass
    {
        Unknown,
        PlasticFFFPrinter,
        MetalSLSPrinter
    }


    public abstract class MachineInfo
    {
        protected static string UnknownUUID = "00000000-0000-0000-0000-000000000000";

        public string ManufacturerName = "Unknown";
        public string ManufacturerUUID = UnknownUUID;
        public string ModelIdentifier = "Machine";
        public string ModelUUID = UnknownUUID;
        public MachineClass Class = MachineClass.Unknown;

        public double BedSizeXMM = 100;
        public double BedSizeYMM = 100;
        public double MaxHeightMM = 100;


        public abstract T CloneAs<T>() where T : class;
        protected virtual void CopyFieldsTo(MachineInfo to)
        {
            to.ManufacturerName = this.ManufacturerName;
            to.ManufacturerUUID = this.ManufacturerUUID;
            to.ModelIdentifier = this.ModelIdentifier;
            to.ModelUUID = this.ModelUUID;
            to.Class = this.Class;
            to.BedSizeXMM = this.BedSizeXMM;
            to.BedSizeYMM = this.BedSizeYMM;
            to.MaxHeightMM = this.MaxHeightMM;
        }
    }



    public class FFFMachineInfo : MachineInfo
    {
        /*
         * printer mechanics
         */
        public double NozzleDiamMM = 0.4;
        public double FilamentDiamMM = 1.75;

        public double MinLayerHeightMM = 0.2;
        public double MaxLayerHeightMM = 0.2;

        /*
         * Temperatures
         */

        public int MinExtruderTempC = 20;
        public int MaxExtruderTempC = 230;

        public bool HasHeatedBed = false;
        public int MinBedTempC = 0;
        public int MaxBedTempC = 0;

        /*
         * All units are mm/min = (mm/s * 60)
         */
        public int MaxExtrudeSpeedMMM = 50 * 60;
        public int MaxTravelSpeedMMM = 100 * 60;
        public int MaxZTravelSpeedMMM = 20 * 60;
        public int MaxRetractSpeedMMM = 20 * 60;

        /*
         * Hacks?
         */

        public double MinPointSpacingMM = 0.5;          // Avoid emitting gcode extrusion points closer than this spacing.
                                                        // This is a workaround for the fact that many printers do not gracefully
                                                        // handle very tiny sequential extrusion steps. This setting could be
                                                        // configured using CalibrationModelGenerator.MakePrintStepSizeTest() with
                                                        // all other cleanup steps disabled.
                                                        // [TODO] this is actually speed-dependent...

        public override T CloneAs<T>()
        {
            FFFMachineInfo fi = new FFFMachineInfo();
            this.CopyFieldsTo(fi);
            return fi as T;
        }
        protected virtual void CopyFieldsTo(FFFMachineInfo to)
        {
            base.CopyFieldsTo(to);

            to.NozzleDiamMM = this.NozzleDiamMM;
            to.FilamentDiamMM = this.FilamentDiamMM;
            to.MinLayerHeightMM = this.MinLayerHeightMM;
            to.MaxHeightMM = this.MaxHeightMM;
            to.MinExtruderTempC = this.MaxExtruderTempC;
            to.HasHeatedBed = this.HasHeatedBed;
            to.MinBedTempC = this.MinBedTempC;
            to.MaxBedTempC = this.MaxBedTempC;
            to.MaxExtrudeSpeedMMM = this.MaxExtrudeSpeedMMM;
            to.MaxTravelSpeedMMM = this.MaxTravelSpeedMMM;
            to.MaxZTravelSpeedMMM = this.MaxZTravelSpeedMMM;
            to.MaxRetractSpeedMMM = this.MaxRetractSpeedMMM;
            to.MinPointSpacingMM = this.MinPointSpacingMM;

            to.ManufacturerName = this.ManufacturerName;
            to.ManufacturerUUID = this.ManufacturerUUID;
            to.ModelIdentifier = this.ModelIdentifier;
            to.ModelUUID = this.ModelUUID;
            to.Class = this.Class;
            to.BedSizeXMM = this.BedSizeXMM;
            to.BedSizeYMM = this.BedSizeYMM;
            to.MaxHeightMM = this.MaxHeightMM;
        }
    }



    public abstract class PlanarAdditiveSettings
	{
        /// <summary>
        /// This is the "name" of this settings (eg user identifier)
        /// </summary>
        public string Identifier = "Default";

		public double LayerHeightMM = 0.2;


        public string ClassTypeName {
            get { return GetType().ToString(); }
        }


        public abstract MachineInfo BaseMachine { get; set; }

        public abstract T CloneAs<T>() where T : class;
        protected virtual void CopyFieldsTo(PlanarAdditiveSettings to)
        {
            to.Identifier = this.Identifier;
            to.LayerHeightMM = this.LayerHeightMM;
        }
    }



    public class SingleMaterialFFFSettings : PlanarAdditiveSettings
	{
        // This is a bit of an odd place for this, but settings are where we actually
        // know what assembler we should be using...
        public virtual AssemblerFactoryF AssemblerType() {
            throw new NotImplementedException("Settings.AssemblerType() not provided");
        }


        protected FFFMachineInfo machineInfo;
        public FFFMachineInfo Machine {
            get { if (machineInfo == null) machineInfo = new FFFMachineInfo(); return machineInfo; }
            set { machineInfo = value; }
        }


        public override MachineInfo BaseMachine {
            get { return Machine; }
            set { if (value is FFFMachineInfo)
                    machineInfo = value as FFFMachineInfo;
                 else
                    throw new Exception("SingleMaterialFFFSettings.Machine.set: type is not FFFMachineInfo!");
            }
        }

        /*
         * Temperatures
         */

        public int ExtruderTempC = 210;
        public int HeatedBedTempC = 0;

		/*
		 * Distances.
		 * All units are mm
		 */

		public double RetractDistanceMM = 1.3;
        public double MinRetractTravelLength = 2.5;     // don't retract if we are travelling less than this distance


		/*
		 * Speeds. 
		 * All units are mm/min = (mm/s * 60)
		 */

		// these are all in units of millimeters/minute
		public double RetractSpeed = 25 * 60;   // 1500

		public double ZTravelSpeed = 23 * 60;   // 1380

		public double RapidTravelSpeed = 150 * 60;  // 9000

		public double CarefulExtrudeSpeed = 30 * 60;  	// 1800
		public double RapidExtrudeSpeed = 90 * 60;      // 5400

		public double OuterPerimeterSpeedX = 0.5;


        /*
         * Shells
         */
        public int Shells = 2;
        public int InteriorSolidRegionShells = 0;       // how many shells to add around interior solid regions (eg roof/floor)

		/*
		 * Roof/Floors
		 */
		public int RoofLayers = 2;
		public int FloorLayers = 2;

        /*
         *  Solid fill settings
         */
        public double SolidFillNozzleDiamStepX = 1.0;       // multipler on Machine.NozzleDiamMM, defines spacing between adjacent
                                                            // solid fill parallel lines
        public double SolidFillBorderOverlapX = 0.25f;      // this is a multiplier on Machine.NozzleDiamMM, defines how far we
                                                            // overlap solid fill onto border shells (if 0, no overlap)

        /*
		 * Sparse infill settings
		 */
        public double SparseLinearInfillStepX = 3.0;      // this is a multiplier on FillPathSpacingMM

        public double SparseFillBorderOverlapX = 0.25f;     // this is a multiplier on Machine.NozzleDiamMM, defines how far we
                                                            // overlap solid fill onto border shells (if 0, no overlap)

        /*
         * Support settings
         */
        public bool EnableSupport = false;
        public double SupportSpacingStepX = 3.0;         // usage depends on support technique?           
        public double SupportVolumeScale = 0.9;          // multiplier on extrusion volume
        public bool EnableSupportShell = true;           // should we print a shell around support areas
        public double SupportGapInLayerX = 1.25f;        // multiple of Machine.NozzleDiam that we leave between support and print

        /*
         * Toolpath filtering options
         */
        public bool ClipSelfOverlaps = false;            // if true, try to remove portions of toolpaths that will self-overlap
        public double SelfOverlapToleranceX = 0.75;      // what counts as 'self-overlap'. this is a multiplier on NozzleDiamMM


        /*
         * Debug/Utility options
         */

        public Interval1i LayerRangeFilter = new Interval1i(0, 999999999);   // only compute slices in this range




        /*
         * functions that calculate derived values
         * NOTE: these cannot be properties because then they will be json-serialized!
         */
        public double SolidFillPathSpacingMM() {
            return Machine.NozzleDiamMM * SolidFillNozzleDiamStepX;
        }



        public override T CloneAs<T>()
        {
            SingleMaterialFFFSettings copy = new SingleMaterialFFFSettings();
            this.CopyFieldsTo(copy);
            return copy as T;
        }
        protected virtual void CopyFieldsTo(SingleMaterialFFFSettings to)
        {
            base.CopyFieldsTo(to);
            to.machineInfo = this.machineInfo.CloneAs<FFFMachineInfo>();

            to.ExtruderTempC = this.ExtruderTempC;
            to.HeatedBedTempC = this.HeatedBedTempC;
            to.SolidFillNozzleDiamStepX = this.SolidFillNozzleDiamStepX;
            to.SolidFillBorderOverlapX = this.SolidFillBorderOverlapX;
            to.RetractDistanceMM = this.RetractDistanceMM;
            to.MinRetractTravelLength = this.MinRetractTravelLength;

            to.RetractSpeed = this.RetractSpeed;
            to.ZTravelSpeed = this.ZTravelSpeed;
            to.RapidTravelSpeed = this.RapidTravelSpeed;
            to.CarefulExtrudeSpeed = this.CarefulExtrudeSpeed;
            to.RapidExtrudeSpeed = this.RapidExtrudeSpeed;
            to.OuterPerimeterSpeedX = this.OuterPerimeterSpeedX;

            to.Shells = this.Shells;
            to.InteriorSolidRegionShells = this.InteriorSolidRegionShells;
            to.RoofLayers = this.RoofLayers;
            to.FloorLayers = this.FloorLayers;
            to.SparseLinearInfillStepX = this.SparseLinearInfillStepX;
            to.SparseFillBorderOverlapX = this.SparseFillBorderOverlapX;
            to.EnableSupport = this.EnableSupport;
            to.SupportSpacingStepX = this.SupportSpacingStepX;
            to.SupportVolumeScale = this.SupportVolumeScale;
            to.ClipSelfOverlaps = this.ClipSelfOverlaps;
            to.SelfOverlapToleranceX = this.SelfOverlapToleranceX;
            to.LayerRangeFilter = this.LayerRangeFilter;
        }

    }




    // just for naming...
    public class GenericRepRapSettings : SingleMaterialFFFSettings
    {
        public override AssemblerFactoryF AssemblerType() {
            return RepRapAssembler.Factory;
        }


        public override T CloneAs<T>()
        {
            GenericRepRapSettings copy = new GenericRepRapSettings();
            this.CopyFieldsTo(copy);
            return copy as T;
        }
        

    }

}