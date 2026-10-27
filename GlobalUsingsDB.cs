global using System;
global using System.Collections.Generic;
global using System.Text;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;
global using System.Diagnostics;

global using BRMDatabase;
//global using BRMDatabase.Platforms;
global using BRMDatabase.Tables;
global using BRMDatabase.Utilities;
global using BRMDatabase.Interfaces;

global using System.Xml.Serialization;
global using System.Text.RegularExpressions;
global using System.Collections.ObjectModel;
global using System.Reflection;

#if WINDOWS
global using NSUrl = System.String;
global using WinRT.Interop;

#endif
#if MACCATALYST || MACOS || IOS

global using NSUrl = Foundation.NSUrl;
#endif
#if ANDROID
global using NSUrl = System.String;
#endif



