// Copyright (c) 2026 Kadmium (Emil Fredrik SjÃ¶stedt). All rights reserved.
// Licensed under the Kadmium Software & Source Code License Agreement.
// See LICENSE.txt in the project root for full license terms.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packer.Core.Interfaces
{

    public interface IPackerConfig
    {
        string AllowedExtensions { get; }
        string IgnoredFolders { get; }
        string ModuleFolderAnchors { get; }
        bool RedactPrivateInformation { get; }
        string CacheLocation { get; }
        int LargeFileWarningThreshold { get; }
        int MaxCharsPerFile { get; }

        string GetActualCachePath();
    }

}