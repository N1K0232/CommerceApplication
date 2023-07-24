using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace CommerceApi.BusinessLayer.Resources
{
    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [DebuggerNonUserCode()]
    [CompilerGenerated()]
    public class Messages
    {
        private static ResourceManager _resourceManager;
        private static CultureInfo _resourceCulture;
        
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Messages()
        {
        }
        
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ResourceManager ResourceManager
        {
            get
            {
                var currentType = typeof(Messages);
                _resourceManager ??= new ResourceManager("CommerceApi.BusinessLayer.Resources.Messages", currentType.Assembly);
                return _resourceManager;
            }
        }
        
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static CultureInfo Culture
        {
            get => _resourceCulture;
            set => _resourceCulture = value;
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The &apos;{PropertyName}&apos; field is required.
        /// </summary>
        public static string FieldRequired
        {
            get
            {
                return ResourceManager.GetString("FieldRequired", _resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Hello, {0}!.
        /// </summary>
        public static string HelloMessage
        {
            get
            {
                return ResourceManager.GetString("HelloMessage", _resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid request!.
        /// </summary>
        public static string ValidationErrors
        {
            get
            {
                return ResourceManager.GetString("ValidationErrors", _resourceCulture);
            }
        }
    }
}