﻿using Askmethat.Aspnet.JsonLocalizer.Extensions;
using Askmethat.Aspnet.JsonLocalizer.Format;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Askmethat.Aspnet.JsonLocalizer.Localizer
{
    internal class JsonStringLocalizer : JsonStringLocalizerBase, IStringLocalizer
    {
        private readonly IHostingEnvironment _env;

        public JsonStringLocalizer(IOptions<JsonLocalizationOptions> localizationOptions, IHostingEnvironment env, string baseName = null) : base(localizationOptions, baseName)
        {
            _env = env;
            _resourcesRelativePath = GetJsonRelativePath(_localizationOptions.Value.ResourcesPath);

            InitJsonStringLocalizer();
        }



        public LocalizedString this[string name]
        {
            get
            {
                string value = GetString(name);
                return new LocalizedString(name, value ?? name, resourceNotFound: value == null);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                string format = GetString(name);
                string value = GetPluralLocalization(name, format, arguments);
                return new LocalizedString(name, value, resourceNotFound: format == null);
            }
        }

        private string GetPluralLocalization(string name, string format, object[] arguments)
        {
            object last = arguments.LastOrDefault();
            string value;
            if (last != null && last is bool isPlural)
            {
                value = GetString(name);
                if (value.Contains(_localizationOptions.Value.PluralSeparator))
                {
                    int index = (isPlural ? 1 : 0);
                    value = value.Split(_localizationOptions.Value.PluralSeparator)[index];
                }
                else
                {
                    value = String.Format(format ?? name, arguments);
                }
            }
            else
            {
                value = String.Format(format ?? name, arguments);
            }

            return value;
        }
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return includeParentCultures ? localization?
                    .Select(
                        l =>
                        {
                            string value = GetString(l.Key);
                            return new LocalizedString(l.Key, value ?? l.Key, resourceNotFound: value == null);
                        }
                    ) :
                    localization?
                    .Where(w => !w.Value.IsParent)
                    .Select(
                        l =>
                        {
                            string value = GetString(l.Key);
                            return new LocalizedString(l.Key, value ?? l.Key, resourceNotFound: value == null);
                        }
                    )
                    ;

        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            return new JsonStringLocalizer(_localizationOptions, _env);
        }

        private string GetString(string name, bool shouldTryDefaultCulture = true)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }


            if (shouldTryDefaultCulture && !IsUICultureCurrentCulture(CultureInfo.CurrentUICulture))
            {
                InitJsonStringLocalizer(CultureInfo.CurrentUICulture);
                AddMissingCultureToSupportedCulture(CultureInfo.CurrentUICulture);
                GetCultureToUse(CultureInfo.CurrentUICulture);
            }

            if (localization != null && localization.TryGetValue(name, out LocalizatedFormat localizedValue))
            {
                return localizedValue.Value;
            }

            if (shouldTryDefaultCulture)
            {
                GetCultureToUse(_localizationOptions.Value.DefaultCulture);
                return GetString(name, false);
            }

            //advert user that current name string does not 
            //contains any translation
            Console.Error.WriteLine($"{name} does not contains any translation");
            return null;
        }

        /// <summary>
        /// Get path of json
        /// </summary>
        /// <returns>JSON relative path</returns>
        private string GetJsonRelativePath(string path)
        {
            string fullPath = string.Empty;
            if (this._localizationOptions.Value.IsAbsolutePath)
            {
                fullPath = path;
            }
            if (!this._localizationOptions.Value.IsAbsolutePath && string.IsNullOrEmpty(path))
            {
                fullPath = Path.Combine(_env.ContentRootPath, "Resources");
            }
            else if (!this._localizationOptions.Value.IsAbsolutePath && !string.IsNullOrEmpty(path))
            {
                fullPath = Path.Combine(AppContext.BaseDirectory,path);
            }
            return fullPath;
        }
    }
}
