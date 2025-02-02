﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace Mapping_Tools.Components.Domain {
    internal class MapPathStringAddNewLinesConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((string)value).Replace('|', '\n');
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((string)value).Replace('\n', '|');
        }
    }
}
