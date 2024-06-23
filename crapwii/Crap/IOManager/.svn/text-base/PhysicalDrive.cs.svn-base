//-------------------------------------
// WBFSSync - WBFSSync.exe
//
// Copyright 2009 Caian (ÔmΣga Frøst) <frost.omega@hotmail.com>
//
// WBFSSync is Licensed under the terms of the Microsoft Reciprocal License (Ms-RL)
//
// PhysicalDrive.cs:
//
// Classe que encapsula informações de um disco físico
//
//-------------------------------------

using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Management;

namespace WBFSSync
{
    public class PhysicalDrive
    {
        //---------------------------- Constantes

        const String WMI_DiskDrive = "Win32_DiskDrive";

        //---------------------------- Variáveis

        public string Manufacturer = "";
        public string Model = "";
        public string Description = "";
        public string InterfaceType = "";
        public string Size = "";
        public string Partitions = "";
        public string ScsiBus = "";
        public string ScsiTargetID = "";
        public string DeviceID = "";

        MBRReader Mbr = null;

        //---------- Enumerador WMI de discos

        public static PhysicalDrive[] Disks = null;

        //---------------------------- Rotinas

        //---------- Enumerador WMI de discos

        //---------- Enumera os drives físicos
        public static int EnumeratePhysicalDrives()
        {
            return EnumeratePhysicalDrives(Environment.MachineName, "", "");
        }

        public static int EnumeratePhysicalDrives(String machineName, String userName, String password)
        {
            String path = @"\\" + Environment.MachineName + @"\root\cimv2";

            ManagementScope managementScope = null;
            if ((userName != string.Empty) && (password != string.Empty))
            {
                ConnectionOptions options = new ConnectionOptions();
                options.Username = "";
                options.Password = "";

                managementScope = new ManagementScope();
                managementScope.Options = options;
                managementScope.Path = new ManagementPath(path);
            }

            ManagementClass managementClass = new ManagementClass(path + ":" + WMI_DiskDrive);
            if (managementScope != null) managementClass.Scope = managementScope;
            ManagementObjectCollection instances = managementClass.GetInstances();
            if (instances == null) return 0;

            //

            Disks = new PhysicalDrive[instances.Count];

            ManagementObjectCollection.ManagementObjectEnumerator enumerator = instances.GetEnumerator();
            int i = 0;
            while (enumerator.MoveNext())
            {
                Disks[i] = new PhysicalDrive();
                ManagementObject instance = (ManagementObject)enumerator.Current;

                Disks[i].Manufacturer = (instance.Properties["Manufacturer"].Value != null)
                               ? instance.Properties["Manufacturer"].Value.ToString().Trim()
                               : string.Empty;
                Disks[i].Model = (instance.Properties["Model"].Value != null)
                            ? instance.Properties["Model"].Value.ToString().Trim()
                            : string.Empty;
                Disks[i].Description = (instance.Properties["Description"].Value != null)
                                  ? instance.Properties["Description"].Value.ToString().Trim()
                                  : string.Empty;
                Disks[i].InterfaceType = (instance.Properties["InterfaceType"].Value != null)
                                    ? instance.Properties["InterfaceType"].Value.ToString().Trim()
                                    : string.Empty;
                Disks[i].Size = (instance.Properties["Size"].Value != null)
                           ? instance.Properties["Size"].Value.ToString().Trim()
                           : string.Empty;
                Disks[i].Partitions = (instance.Properties["Partitions"].Value != null)
                                 ? instance.Properties["Partitions"].Value.ToString().Trim()
                                 : string.Empty;
                Disks[i].ScsiBus = (instance.Properties["ScsiBus"].Value != null)
                              ? instance.Properties["ScsiBus"].Value.ToString().Trim()
                              : string.Empty;
                Disks[i].ScsiTargetID = (instance.Properties["ScsiTargetID"].Value != null)
                                   ? instance.Properties["ScsiTargetID"].Value.ToString().Trim()
                                   : string.Empty;
                Disks[i].DeviceID = (instance.Properties["DeviceID"].Value != null)
                               ? instance.Properties["DeviceID"].Value.ToString().Trim()
                               : string.Empty;

                i++;
            }

            return instances.Count;
        }
    }
}
