﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Scans;
using Chemistry;

namespace MetaLive
{
    public class BoxCarScan
    {
        //public static string[] StaticBoxCar_2_12_Scan = new string[2]
        //{ "[(400, 423.2), (441.2, 459.9), (476.3, 494.3), (510.3, 528.8), (545, 563.8), (580.8, 600.3), (618.4, 639.8), " +
        //        "(660.3, 684.3), (708.3, 735.4), (764.4, 799.9),(837.9, 885.4), (945, 1032)]",
        //"[(422.2,442.2), (458.9,477.3), (493.3,511.3), (527.8,546), (562.8,581.8), (599.3, 619.4), (638.8, 661.3), " +
        //        "(683.3, 709.3), (734.4, 765.4), (798.9, 838.9), (884.4, 946), (1031, 1200)]"
        //};

        public static string[] StaticBoxCar_2_12_Scan { get; set; }
        public static string StaticBoxCarDynamicTargets { get; set; }
        public static string StaticBoxCarDynamicMaxIts { get; set; }

        public static void PlaceBoxCarScan(IScans m_scans, Parameters parameters)
        {
            if (m_scans.PossibleParameters.Length == 0)
            {
                return;
            }

            ICustomScan scan = m_scans.CreateCustomScan();
            scan.Values["FirstMass"] = parameters.BoxCarScanSetting.BoxCarMzRangeLowBound.ToString();
            scan.Values["LastMass"] = parameters.BoxCarScanSetting.BoxCarMzRangeHighBound.ToString();
            scan.Values["IsolationRangeLow"] = (parameters.BoxCarScanSetting.BoxCarMzRangeLowBound).ToString();
            scan.Values["IsolationRangeHigh"] = (parameters.BoxCarScanSetting.BoxCarMzRangeHighBound).ToString();

            scan.Values["MaxIT"] = parameters.BoxCarScanSetting.BoxCarMaxInjectTimeInMillisecond.ToString();
            scan.Values["Resolution"] = parameters.BoxCarScanSetting.BoxCarResolution.ToString();
            scan.Values["Polarity"] = parameters.GeneralSetting.Polarity.ToString();
            scan.Values["NCE"] = "0.0";
            scan.Values["NCE_NormCharge"] = parameters.BoxCarScanSetting.BoxCarNormCharge.ToString();
            scan.Values["NCE_SteppedEnergy"] = "0";
            scan.Values["NCE_Factors"] = "[]";

            scan.Values["SourceCID"] = parameters.GeneralSetting.SourceCID.ToString("0.00");
            scan.Values["Microscans"] = parameters.BoxCarScanSetting.BoxCarMicroScans.ToString();
            scan.Values["AGC_Target"] = parameters.BoxCarScanSetting.BoxCarAgcTarget.ToString();
            scan.Values["AGC_Mode"] = parameters.GeneralSetting.AGC_Mode.ToString();

            scan.Values["MsxInjectTargets"] = StaticBoxCarDynamicTargets;
            scan.Values["MsxInjectMaxITs"] = StaticBoxCarDynamicMaxIts;
            scan.Values["MsxInjectNCEs"] = "[]";
            scan.Values["MsxInjectDirectCEs"] = "[]";
            for (int i = 0; i < parameters.BoxCarScanSetting.BoxCarScans; i++)
            {               
                scan.Values["MsxInjectRanges"] = StaticBoxCar_2_12_Scan[i];
              
                Console.WriteLine("{0:HH:mm:ss,fff} placing BoxCar MS1 scan", DateTime.Now);
                m_scans.SetCustomScan(scan);
            }
        }

        public static Tuple<double, double, double>[] GenerateBoxes(List<double> mzs)
        {
            Tuple<double, double, double>[] ranges = new Tuple<double, double, double>[mzs.Count];

            for (int i = 1; i < mzs.Count; i++)
            {
                ranges[i - 1] = new Tuple<double, double, double>(mzs[i - 1], mzs[i], mzs[i] - mzs[i - 1]);
            }
            ranges[mzs.Count - 1] = new Tuple<double, double, double>(mzs.Last(), 2000, 2000 - mzs.Last());

            return ranges.OrderByDescending(p => p.Item3).Where(p => p.Item3 > 15).Take(12).OrderBy(p => p.Item1).ToArray();
        }

        public static void PlaceBoxCarScan(IScans m_scans, Parameters parameters, Tuple<double, double, double>[] dynamicBox)
        {
            if (m_scans.PossibleParameters.Length == 0)
            {
                return;
            }

            ICustomScan scan = m_scans.CreateCustomScan();
            scan.Values["FirstMass"] = parameters.BoxCarScanSetting.BoxCarMzRangeLowBound.ToString();
            scan.Values["LastMass"] = parameters.BoxCarScanSetting.BoxCarMzRangeHighBound.ToString();
            scan.Values["IsolationRangeLow"] = (parameters.BoxCarScanSetting.BoxCarMzRangeLowBound - 200).ToString();
            scan.Values["IsolationRangeHigh"] = (parameters.BoxCarScanSetting.BoxCarMzRangeHighBound + 200).ToString();

            scan.Values["MaxIT"] = parameters.BoxCarScanSetting.BoxCarMaxInjectTimeInMillisecond.ToString();
            scan.Values["Resolution"] = parameters.BoxCarScanSetting.BoxCarResolution.ToString();
            scan.Values["Polarity"] = parameters.GeneralSetting.Polarity.ToString();
            scan.Values["NCE"] = "0.0";
            scan.Values["NCE_NormCharge"] = parameters.BoxCarScanSetting.BoxCarNormCharge.ToString();
            scan.Values["NCE_SteppedEnergy"] = "0";
            scan.Values["NCE_Factors"] = "[]";

            scan.Values["SourceCID"] = parameters.GeneralSetting.SourceCID.ToString("0.00");
            scan.Values["Microscans"] = parameters.BoxCarScanSetting.BoxCarMicroScans.ToString();
            scan.Values["AGC_Target"] = parameters.BoxCarScanSetting.BoxCarAgcTarget.ToString();
            scan.Values["AGC_Mode"] = parameters.GeneralSetting.AGC_Mode.ToString();

            

            string dynamicTargets;
            string dynamicMaxIts;
            var dynamicBoxString = BuildDynamicBoxString(parameters, dynamicBox, out dynamicTargets, out dynamicMaxIts);
            scan.Values["MsxInjectRanges"] = dynamicBoxString;
            scan.Values["MsxInjectTargets"] = dynamicTargets;
            scan.Values["MsxInjectMaxITs"] = dynamicMaxIts;

            scan.Values["MsxInjectNCEs"] = "[]";
            scan.Values["MsxInjectDirectCEs"] = "[]";

            Console.WriteLine("{0:HH:mm:ss,fff} placing Dynamic BoxCar MS1 scan {1}", DateTime.Now, dynamicBoxString);
            m_scans.SetCustomScan(scan);

        }

        public static string BuildDynamicBoxString(Parameters parameters, Tuple<double, double, double>[] dynamicBox, out string dynamicBoxTargets, out string dynamicBoxMaxITs)
        {
            string dynamicBoxRanges = "[";

            foreach (var box in dynamicBox)
            {
                dynamicBoxRanges += "(";
                dynamicBoxRanges += (box.Item1+2.5).ToString("0.00");
                dynamicBoxRanges += ",";
                dynamicBoxRanges += (box.Item2-2.5).ToString("0.00");
                dynamicBoxRanges += "),";
            }

            dynamicBoxRanges.Remove(0, dynamicBoxRanges.Count()-1);
            dynamicBoxRanges += "]";


            //Boxtargets and BoxMaxITs
            dynamicBoxTargets = "[";
            for (int i = 0; i < dynamicBox.Length; i++)
            {
                dynamicBoxTargets += parameters.BoxCarScanSetting.BoxCarAgcTarget / dynamicBox.Length;
                if (i != dynamicBox.Length-1)
                {
                    dynamicBoxTargets += ",";
                }
            }
            dynamicBoxTargets += "]";

            dynamicBoxMaxITs = "[";
            for (int i = 0; i < dynamicBox.Length; i++)
            {
                dynamicBoxMaxITs += parameters.BoxCarScanSetting.BoxCarMaxInjectTimeInMillisecond / dynamicBox.Length;
                if (i != dynamicBox.Length - 1)
                {
                    dynamicBoxMaxITs += ",";
                }
            }
            dynamicBoxMaxITs += "]";

            return dynamicBoxRanges;
        }

        public static string BuildDynamicBoxInclusionString(Parameters parameters, List<double> dynamicBoxBeforeOrder, out string dynamicBoxTargets, out string dynamicBoxMaxITs)
        {
            //The dynamicBox list should be ordered?
            var dynamicBox = dynamicBoxBeforeOrder.Where(p => p <= parameters.BoxCarScanSetting.BoxCarMzRangeHighBound && p >= parameters.BoxCarScanSetting.BoxCarMzRangeLowBound).ToList();

            string dynamicBoxRanges = "[";

            dynamicBoxRanges += "(";
            
            var firstMass = (dynamicBox[0] - parameters.BoxCarScanSetting.DynamicBlockSize < parameters.BoxCarScanSetting.BoxCarMzRangeLowBound) ? 
                parameters.BoxCarScanSetting.BoxCarMzRangeLowBound : dynamicBox[0] - parameters.BoxCarScanSetting.DynamicBlockSize;
            dynamicBoxRanges += firstMass.ToString("0.000");
            dynamicBoxRanges += ",";
            dynamicBoxRanges += (dynamicBox[0] + parameters.BoxCarScanSetting.DynamicBlockSize).ToString("0.000");
            dynamicBoxRanges += "),";

            for (int i = 1; i < dynamicBox.Count-1; i++)
            {
                dynamicBoxRanges += "(";
                dynamicBoxRanges += (dynamicBox[i] - parameters.BoxCarScanSetting.DynamicBlockSize).ToString("0.000");
                dynamicBoxRanges += ",";
                dynamicBoxRanges += (dynamicBox[i] + parameters.BoxCarScanSetting.DynamicBlockSize).ToString("0.000");
                dynamicBoxRanges += "),";
            }

            dynamicBoxRanges += "(";
            dynamicBoxRanges += (dynamicBox.Last() - parameters.BoxCarScanSetting.DynamicBlockSize).ToString("0.000");
            dynamicBoxRanges += ",";

            var lastMass = (dynamicBox.Last() + parameters.BoxCarScanSetting.DynamicBlockSize > parameters.BoxCarScanSetting.BoxCarMzRangeHighBound) ? 
                parameters.BoxCarScanSetting.BoxCarMzRangeHighBound : dynamicBox.Last() + parameters.BoxCarScanSetting.DynamicBlockSize;
            dynamicBoxRanges += lastMass.ToString("0.000");
            dynamicBoxRanges += ")";

            dynamicBoxRanges += "]";


            //Boxtargets and BoxMaxITs
            dynamicBoxTargets = "[";
            for (int i = 0; i < dynamicBox.Count; i++)
            {
                dynamicBoxTargets += parameters.BoxCarScanSetting.BoxCarAgcTarget / dynamicBox.Count;
                if (i != dynamicBox.Count - 1)
                {
                    dynamicBoxTargets += ",";
                }
            }
            dynamicBoxTargets += "]";

            dynamicBoxMaxITs = "[";
            for (int i = 0; i < dynamicBox.Count; i++)
            {
                dynamicBoxMaxITs += parameters.BoxCarScanSetting.BoxCarMaxInjectTimeInMillisecond / dynamicBox.Count;
                if (i != dynamicBox.Count - 1)
                {
                    dynamicBoxMaxITs += ",";
                }
            }
            dynamicBoxMaxITs += "]";

            return dynamicBoxRanges;
        }

    }
}
