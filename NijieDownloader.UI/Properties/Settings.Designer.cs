﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NijieDownloader.UI.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.9.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string Username {
            get {
                return ((string)(this["Username"]));
            }
            set {
                this["Username"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string Password {
            get {
                return ((string)(this["Password"]));
            }
            set {
                this["Password"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("D:\\Temp\\Nijie")]
        public string RootDirectory {
            get {
                return ((string)(this["RootDirectory"]));
            }
            set {
                this["RootDirectory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("{memberId}\\{imageId} - {tags}")]
        public string FilenameFormat {
            get {
                return ((string)(this["FilenameFormat"]));
            }
            set {
                this["FilenameFormat"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool UpdateRequired {
            get {
                return ((bool)(this["UpdateRequired"]));
            }
            set {
                this["UpdateRequired"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("100")]
        public string cacheMemoryLimitMegabytes {
            get {
                return ((string)(this["cacheMemoryLimitMegabytes"]));
            }
            set {
                this["cacheMemoryLimitMegabytes"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool AutoLogin {
            get {
                return ((bool)(this["AutoLogin"]));
            }
            set {
                this["AutoLogin"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool AutoSaveBatchList {
            get {
                return ((bool)(this["AutoSaveBatchList"]));
            }
            set {
                this["AutoSaveBatchList"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int ConcurrentImageLoad {
            get {
                return ((int)(this["ConcurrentImageLoad"]));
            }
            set {
                this["ConcurrentImageLoad"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int ConcurrentJob {
            get {
                return ((int)(this["ConcurrentJob"]));
            }
            set {
                this["ConcurrentJob"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2000")]
        public int JobDelay {
            get {
                return ((int)(this["JobDelay"]));
            }
            set {
                this["JobDelay"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool TraceDB {
            get {
                return ((bool)(this["TraceDB"]));
            }
            set {
                this["TraceDB"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool DownloadAvatar {
            get {
                return ((bool)(this["DownloadAvatar"]));
            }
            set {
                this["DownloadAvatar"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("{memberId}\\folder.jpg")]
        public string AvatarFilenameFormat {
            get {
                return ((string)(this["AvatarFilenameFormat"]));
            }
            set {
                this["AvatarFilenameFormat"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("{memberId}\\{imageId}{page} - {tags}")]
        public string MangaFilenameFormat {
            get {
                return ((string)(this["MangaFilenameFormat"]));
            }
            set {
                this["MangaFilenameFormat"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool SaveDB {
            get {
                return ((bool)(this["SaveDB"]));
            }
            set {
                this["SaveDB"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Debug")]
        public string LogLevel {
            get {
                return ((string)(this["LogLevel"]));
            }
            set {
                this["LogLevel"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool EnableCompression {
            get {
                return ((bool)(this["EnableCompression"]));
            }
            set {
                this["EnableCompression"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool LoadThumbnail {
            get {
                return ((bool)(this["LoadThumbnail"]));
            }
            set {
                this["LoadThumbnail"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool SkipIfExistsInDB {
            get {
                return ((bool)(this["SkipIfExistsInDB"]));
            }
            set {
                this["SkipIfExistsInDB"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Member")]
        public string StartPage {
            get {
                return ((string)(this["StartPage"]));
            }
            set {
                this["StartPage"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool DumpDownloadedImagesToTextFile {
            get {
                return ((bool)(this["DumpDownloadedImagesToTextFile"]));
            }
            set {
                this["DumpDownloadedImagesToTextFile"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Collections.Specialized.StringCollection FormatList {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["FormatList"]));
            }
            set {
                this["FormatList"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("8")]
        public int FormatListCount {
            get {
                return ((int)(this["FormatListCount"]));
            }
            set {
                this["FormatListCount"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool PreventSleep {
            get {
                return ((bool)(this["PreventSleep"]));
            }
            set {
                this["PreventSleep"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool UseServerDate {
            get {
                return ((bool)(this["UseServerDate"]));
            }
            set {
                this["UseServerDate"] = value;
            }
        }
    }
}
