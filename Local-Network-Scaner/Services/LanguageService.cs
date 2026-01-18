using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Local_Network_Scanner.Services
{
    public class LanguageService
    {
        public void ChangeLanguage(string languageCode)
        {
            ResourceDictionary dict = new ResourceDictionary();
            switch (languageCode)
            {
                case "en":
                    dict.Source = new Uri("Resources/Languages/StringResources.en.xaml", UriKind.Relative);
                    break;
                case "pl":
                    dict.Source = new Uri("Resources/Languages/StringResources.pl.xaml", UriKind.Relative);
                    break;
                case "uk":
                    dict.Source = new Uri("Resources/Languages/StringResources.uk.xaml", UriKind.Relative);
                    break;
                case "ru":
                    dict.Source = new Uri("Resources/Languages/StringResources.ru.xaml", UriKind.Relative);
                    break;
                case "no":
                    dict.Source = new Uri("Resources/Languages/StringResources.no.xaml", UriKind.Relative);
                    break;
                default:
                    dict.Source = new Uri("Resources/Languages/StringResources.en.xaml", UriKind.Relative);
                    break;
            }

            // Find the old language dictionary and replace it
            var mergedDicts = Application.Current.Resources.MergedDictionaries;
            for (int i = 0; i < mergedDicts.Count; i++)
            {
                if (mergedDicts[i].Source != null && mergedDicts[i].Source.OriginalString.Contains("StringResources"))
                {
                    mergedDicts[i] = dict;
                    return;
                }
            }
            // If not found, just add it
            mergedDicts.Add(dict);
        }
    }
}
