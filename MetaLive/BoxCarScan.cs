﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Scans;
using Chemistry;

namespace MetaLive
{
    class BoxCarScan
    {
        public static void PlaceBoxCarScan(IScans m_scans, Parameters parameters)
        {
            if (m_scans.PossibleParameters.Length == 0)
            {
                return;
            }

            ICustomScan scan = m_scans.CreateCustomScan();
            scan.Values["Resolution"] = parameters.BoxCarScanSetting.BoxCarResolution.ToString();
            scan.Values["FirstMass"] = parameters.BoxCarScanSetting.BoxCarMzRangeLowBound.ToString();
            scan.Values["LastMass"] = parameters.BoxCarScanSetting.BoxCarMzRangeHighBound.ToString();
            scan.Values["MaxIT"] = parameters.BoxCarScanSetting.BoxCarMaxInjectTimeInMillisecond.ToString();
            scan.Values["NCE_NormCharge"] = parameters.BoxCarScanSetting.BoxCarNormCharge.ToString();
            scan.Values["AGC_Target"] = parameters.BoxCarScanSetting.BoxCarAgcTarget.ToString();
            for (int i = 0; i < parameters.BoxCarScanSetting.BoxCarScans; i++)
            {
                scan.Values["MsxInjectRanges"] = parameters.BoxCarScanSetting.BoxCarMsxInjectRanges[i];

                Console.WriteLine("{0:HH:mm:ss,fff} placing MS1 scan", DateTime.Now);
                m_scans.SetCustomScan(scan);
            }           
        }

        public static void PlaceBoxCarScan(IScans m_scans, Parameters parameters, List<double> dynamicBox)
        {
            if (m_scans.PossibleParameters.Length == 0)
            {
                return;
            }

            ICustomScan scan = m_scans.CreateCustomScan();
            scan.Values["Resolution"] = parameters.BoxCarScanSetting.BoxCarResolution.ToString();
            scan.Values["FirstMass"] = parameters.BoxCarScanSetting.BoxCarMzRangeLowBound.ToString();
            scan.Values["LastMass"] = parameters.BoxCarScanSetting.BoxCarMzRangeHighBound.ToString();
            scan.Values["MaxIT"] = parameters.BoxCarScanSetting.BoxCarMaxInjectTimeInMillisecond.ToString();
            scan.Values["NCE_NormCharge"] = parameters.BoxCarScanSetting.BoxCarNormCharge.ToString();
            scan.Values["AGC_Target"] = parameters.BoxCarScanSetting.BoxCarAgcTarget.ToString();

            

            scan.Values["MsxInjectRanges"] = BuildDynamicBoxString(parameters, dynamicBox);

            Console.WriteLine("{0:HH:mm:ss,fff} placing MS1 scan", DateTime.Now);
            m_scans.SetCustomScan(scan);

        }

        public static string BuildDynamicBoxString(Parameters parameters, List<double> dynamicBox)
        {
            string dynamicBoxRanges = "[";
            List<double> mzs = new List<double>();
            foreach (var range in dynamicBox)
            {

                for (int i = 0; i < 60; i++)
                {
                    mzs.Add(range.ToMz(i));
                }
            }
            var mzsFiltered = mzs.Where(p => p > parameters.BoxCarScanSetting.BoxCarMzRangeLowBound && p < parameters.BoxCarScanSetting.BoxCarMzRangeHighBound).OrderBy(p => p).ToList();

            for (int i = 0; i < mzsFiltered.Count; i++)
            {
                var mz = mzsFiltered[i];
                if (i == 0)
                {
                    dynamicBoxRanges += "(";
                    dynamicBoxRanges += parameters.BoxCarScanSetting.BoxCarMzRangeLowBound.ToString("0.00");
                    dynamicBoxRanges += ",";
                    dynamicBoxRanges += (mz - 5).ToString("0.00");
                    dynamicBoxRanges += "),";
                }
                else if (i == mzsFiltered.Count - 1)
                {
                    dynamicBoxRanges += "(";              
                    dynamicBoxRanges += (mz + 5).ToString("0.00");
                    dynamicBoxRanges += ",";
                    dynamicBoxRanges += parameters.BoxCarScanSetting.BoxCarMzRangeHighBound.ToString("0.00");
                    dynamicBoxRanges += ")";
                }
                else
                {
                    var mz_front = mzsFiltered[i-1];
                    dynamicBoxRanges += "(";
                    dynamicBoxRanges += (mz_front + 5).ToString("0.00");
                    dynamicBoxRanges += ",";
                    dynamicBoxRanges += (mz + 5).ToString("0.00");
                    dynamicBoxRanges += "),";
                }   

            }
            dynamicBoxRanges += "]";

            return dynamicBoxRanges;
        }
    }
}
