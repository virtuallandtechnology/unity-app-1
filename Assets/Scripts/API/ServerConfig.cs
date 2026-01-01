using Best.HTTP;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static ApiClient;



[Serializable]
public class ForceUpdateData
{
    [Serializable]
    public class VersionParser
    {
        private int _majorVersion;
        private int _minorversion;
        private int _buildNumber;
        public string Version;

        public VersionParser(string version)
        {
            string[] VersionSplit = version.Split('.');
            var s_majorVersion = GetAsDigitCount(VersionSplit[0], 4);
            var s_minorversion = VersionSplit.Length > 1 ? GetAsDigitCount(VersionSplit[1], 4) : "0";
            var s_buildNumber = VersionSplit.Length > 2 ? GetAsDigitCount(VersionSplit[2], 4) : "0";

            _majorVersion = int.Parse(s_majorVersion);
            _minorversion = VersionSplit.Length > 1 ? int.Parse(s_minorversion) : 0;
            _buildNumber = VersionSplit.Length > 2 ? int.Parse(s_buildNumber) : 0;


            int _majorVern = int.Parse(VersionSplit[0]);
            int _minorver = VersionSplit.Length > 1 ? int.Parse(VersionSplit[1]) : 0;
            int _buildNum = VersionSplit.Length > 2 ? int.Parse(VersionSplit[2]) : 0;
            Version = $"{_majorVern}.{_minorver}.{_buildNum}";

        }

        public string GetAsDigitCount(string number, int count)
        {
            string s1 = number;
            if (number.Length < count)
                for (int i = 0; i < count - number.Length; i++)
                    s1 += "0";
            return s1;
        }

        public static bool operator >(VersionParser a, VersionParser b)
        {
            if (a._majorVersion != b._majorVersion)
                return a._majorVersion > b._majorVersion;
            if (a._minorversion != b._minorversion)
                return a._minorversion > b._minorversion;
            return a._buildNumber > b._buildNumber;
        }

        public static bool operator <(VersionParser a, VersionParser b)
        {
            if (a._majorVersion != b._majorVersion)
                return a._majorVersion < b._majorVersion;
            if (a._minorversion != b._minorversion)
                return a._minorversion < b._minorversion;
            return a._buildNumber < b._buildNumber;
        }
        public static bool operator >=(VersionParser a, VersionParser b)
        {
            return !(a < b); 
        }

        public static bool operator <=(VersionParser a, VersionParser b)
        {
            return !(a > b); 
        }
    }


    [SerializeField] private VersionParser _serverCurrentVersionData;
    [SerializeField] private VersionParser _serverForceUpdateVersionData;
    [SerializeField] private VersionParser _clientVersionData;
    public string UpdateUrl;
    public bool CanUpdate;
    public bool ForceUpdate;
    public bool NoUpdate;


    public string description;


    public ForceUpdateData(VERSIONINFO serverData, string clientVersion)
    {

        UpdateUrl = serverData.latest.download;
        description = serverData.latest.description;
        _serverCurrentVersionData = new VersionParser(serverData.latest.version);
        _serverForceUpdateVersionData = new VersionParser(serverData.force_update);
        _clientVersionData = new VersionParser(clientVersion);
        CanUpdate = (_clientVersionData < _serverCurrentVersionData);
        ForceUpdate = (_clientVersionData < _serverForceUpdateVersionData);
        NoUpdate = (_clientVersionData >= _serverForceUpdateVersionData);

    }

    public string GetCurrentVersion => _clientVersionData.Version;

    public string GetServerVersion()
    {
        return ForceUpdate ? _serverForceUpdateVersionData.Version : _serverCurrentVersionData.Version;
    }
}