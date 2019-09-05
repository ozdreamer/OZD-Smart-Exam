namespace QuestionGenerator.UI.Resources.Converters
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;

    using DevExpress.Utils.Svg;
    using DevExpress.Xpf.Core.Native;

    /// <summary>
    /// The class to convert URI to SVG.
    /// </summary>
    /// <seealso cref="System.Windows.Markup.MarkupExtension" />
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class SvgImageSourceConverterExtension : MarkupExtension, IValueConverter
    {
        /// <summary>
        /// The base URI
        /// </summary>
        private readonly Uri baseUri;

        /// <summary>
        /// The URI converter
        /// </summary>
        private readonly UriTypeConverter uriConverter;

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgImageSourceConverterExtension"/> class.
        /// </summary>
        public SvgImageSourceConverterExtension() : this(null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgImageSourceConverterExtension"/> class.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        public SvgImageSourceConverterExtension(Uri baseUri)
        {
            this.baseUri = baseUri;
            uriConverter = new UriTypeConverter();
        }

        /// <summary>
        /// Gets the stream.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns></returns>
        public static Stream GetStream(Uri uri)
        {
            return Application.GetResourceStream(uri).Stream;
        }

        /// <summary>
        /// When implemented in a derived class, returns an object that is provided as the value of the target property for this markup extension.
        /// </summary>
        /// <param name="serviceProvider">A service provider helper that can provide services for the markup extension.</param>
        /// <returns>
        /// The object value to set on the property where the extension is applied.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new SvgImageSourceConverterExtension((serviceProvider.GetService(typeof(IUriContext)) as IUriContext).BaseUri);
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var uri = uriConverter.ConvertFrom(value) as Uri;
            if (uri == null)
            {
                return null;
            }

            var absoluteUri = uri.IsAbsoluteUri ? uri : new Uri(baseUri, uri);
            using (var stream = GetStream(absoluteUri))
            {
                var image = SvgLoader.LoadFromStream(stream);
                return WpfSvgRenderer.CreateImageSource(image, 1d, null, null, true);
            }
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}