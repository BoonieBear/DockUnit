﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Net.Configuration;
using System.Text;

namespace BoonieBear.DeckUnit.SysResourceLib
{

   public interface IPcResources 
    {
        double GetMemoryUsage();
        double GetDiskUsage();
        string GetMacAddress();
        string GetIpAddress();
    }

   public interface IMspResources
    {
        
    }


    public class GetSystemInfo
    {
        public static IPcResources GreateResources()
        {
            IPcResources ir = new GetSysInfo();
            return ir;
        }
        public static IMspResources GreatemsMspResources()
        {
            IMspResources ir = new GetMspInfo();
            return ir;
        }
    }

    
}