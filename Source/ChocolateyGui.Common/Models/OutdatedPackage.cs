﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Chocolatey" file="OutdatedPackage.cs">
//   Copyright 2014 - Present Rob Reynolds, the maintainers of Chocolatey, and RealDimensions Software, LLC
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Xml.Serialization;
using NuGet;

namespace ChocolateyGui.Common.Models
{
    public class OutdatedPackage
    {
        public string Id { get; set; }

        public string VersionString { get; set;  }

        [XmlIgnore]
        public SemanticVersion Version
        {
            get { return new SemanticVersion(VersionString); }
        }
    }
}