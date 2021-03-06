﻿using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using XamlMagic.Service.Reorder;

namespace XamlMagic.Service.Options
{
    public class StylerOptions : IStylerOptions
    {
        private const string DefaultOptionsPath = "XamlMagic.Service.Options.DefaultSettings.json";

        private readonly string[] DefaultAttributeOrderingRuleGroups = new string[]
        {
            // Class definition group
            "x:Class*",
            // WPF Namespaces group
            "xmlns, xmlns:x",
            // Other namespace
            "xmlns:*",
            // Element key and name group
            "x:Key, Key, x:Name, Name, x:Uid, Uid, Title",
            // Attached layout group
            "Grid.Row, Grid.RowSpan, Grid.Column, Grid.ColumnSpan, Canvas.Left, Canvas.Top, Canvas.Right, Canvas.Bottom",
            // Core layout group
            "Width, Height, MinWidth, MinHeight, MaxWidth, MaxHeight",
            // Alignment layout group
            "Margin, Padding, HorizontalAlignment, VerticalAlignment, HorizontalContentAlignment, VerticalContentAlignment, Panel.ZIndex",
            // Visual styling group
            "Style, Background, Foreground, Fill, BorderBrush, BorderThickness, Stroke, StrokeThickness, Opacity",
            // Font property group
            "FontFamily, FontSize, LineHeight, FontWeight, FontStyle, FontStretch",
            // Unmatched
            "*:*, *",
            // Miscellaneous/Other attributes group
            "PageSource, PageIndex, Offset, Color, TargetName, Property, Value, StartPoint, EndPoint",
            // Blend-related group
            "mc:Ignorable, d:IsDataSource, d:LayoutOverrides, d:IsStaticText",
        };

        public StylerOptions()
        {
            this.InitializeProperties();
        }

        /// <summary>
        /// Constructor that accepts an external configuration path before initializing settings.
        /// </summary>
        /// <param name="config">Path to external configuration file.</param>
        public StylerOptions(string config = "")
        {
            if (!String.IsNullOrWhiteSpace(config) && File.Exists(config))
            {
                this.ConfigPath = config;
            }

            this.InitializeProperties();
        }

        /// <summary>
        /// JSON Constructor required to prevent an infinite loop during deserialization.
        /// </summary>
        /// <param name="isJsonConstructor">Dummy parameter to differentiate from default constructor.</param>
        [JsonConstructor]
        public StylerOptions(bool isJsonConstructor = true) { }

        //Indentation

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(4)]
        [JsonProperty("IndentSize", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [Browsable(false)]
        public int IndentSize { get; set; } = 4;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(false)]
        [Browsable(false)]
        [JsonIgnore]
        public bool IndentWithTabs { get; set; }

        // Attribute formatting

        [Category("Attribute Formatting")]
        [DisplayName("Attribute tolerance")]
        [JsonProperty("AttributesTolerance", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [Description("Defines the maximum number of attributes allowed on a single line. If the number of attributes exceeds this value, XAML Magic will break the attributes up across multiple lines. A value of less than 1 means always break up the attributes.\r\n\r\nDefault Value: 2")]
        [DefaultValue(2)]
        public int AttributesTolerance { get; set; }

        [Category("Attribute Formatting")]
        [DisplayName("Keep First Attribute On Same Line")]
        [JsonProperty("KeepFirstAttributeOnSameLine", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [Description("Defines whether the first line of attribute(s) should appear on the same line as the element's start tag.\r\n\r\nDefault Value: false")]
        [DefaultValue(false)]
        public bool KeepFirstAttributeOnSameLine { get; set; }

        [Category("Attribute Formatting")]
        [DisplayName("Max Attribute Characters Per Line")]
        [JsonProperty("MaxAttributeCharatersPerLine", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [Description("Defines the maximum character length of attributes an element can have on each line after the start tag (not including indentation characters). A value of less than 1 means no limit. Note: This setting only takes effect if Max Attributes Per Line is greater than 1 and Attribute Tolerance is greater than 2.\r\n\r\nDefault Value: 0")]
        [DefaultValue(0)]
        public int MaxAttributeCharatersPerLine { get; set; }

        [Category("Attribute Formatting")]
        [DisplayName("Max Attributes Per Line")]
        [JsonProperty("MaxAttributesPerLine", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [Description("Defines the maximum number of attributes an element can have on each line after the start tag if the number of attributes exceeds the attribute tolerance. A value of less than 1 means no limit.\r\n\r\nDefault Value: 1")]
        [DefaultValue(1)]
        public int MaxAttributesPerLine { get; set; }

        [Category("Attribute Formatting")]
        [DisplayName("Newline Exemption Elements")]
        [JsonProperty("NewlineExemptionElements", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [Description("Defines a list of elements whose attributes should not be broken across lines.\r\n\r\nDefault Value: RadialGradientBrush, GradientStop, LinearGradientBrush, ScaleTransfom, SkewTransform, RotateTransform, TranslateTransform, Trigger, Setter")]
        [DefaultValue("RadialGradientBrush, GradientStop, LinearGradientBrush, ScaleTransfom, SkewTransform, RotateTransform, TranslateTransform, Trigger, Condition, Setter")]
        public string NewlineExemptionElements { get; set; }

        [Category("Attribute Formatting")]
        [DisplayName("Separate By Groups")]
        [JsonProperty("SeparateByGroups", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [Description("Defines whether attributes belonging to different rule groups should be put on separate lines, while, if possible, keeping attributes in the same group on the same line.\r\n\r\nDefault Value: false")]
        [DefaultValue(false)]
        public bool SeparateByGroups { get; set; }

        // Attribute Reordering

        [Category("Attribute Reordering")]
        [DisplayName("Enable Attribute Reordering")]
        [JsonProperty("EnableAttributeReordering", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [Description("Defines whether attributes should be reordered. If false, attributes will not be reordered in any way.\r\n\r\nDefault Value: true")]
        [DefaultValue(true)]
        public bool EnableAttributeReordering { get; set; }

        [Category("Attribute Reordering")]
        [DisplayName("Attribute Ordering Rule Groups")]
        [JsonProperty("AttributeOrderingRuleGroups")]
        [Description("Defines attribute ordering rule groups. Each string element is one group. Use ',' as a delimiter between attributes. 'DOS' wildcards are allowed. XAML Magic will order attributes in groups from top to bottom, and within groups left to right.")]
        public string[] AttributeOrderingRuleGroups { get; set; }

        private string serializedAttributeOrderingRuleGroups;

        /// <summary>
        /// We must serialize AttributeOrderingRuleGroups in order for Visual Studio to support exporting this
        /// setting. This property should not be used in JSON configuration processing.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue("")]
        [Browsable(false)]
        [JsonIgnore]
        public string SerializedAttributeOrderingRuleGroups
        {
            get
            {
                return JsonConvert.SerializeObject(this.AttributeOrderingRuleGroups);
            }
            set
            {
                if (this.serializedAttributeOrderingRuleGroups != value)
                {
                    this.serializedAttributeOrderingRuleGroups = value;
                    this.AttributeOrderingRuleGroups = JsonConvert.DeserializeObject<string[]>(value);
                }
            }
        }

        [Category("Attribute Reordering")]
        [DisplayName("Order Attributes By Name")]
        [JsonProperty("OrderAttributesByName", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [Description("Defines whether attributes should be ordered by name if not determined by a rule.\r\n\r\nDefault Value: true")]
        [DefaultValue(true)]
        public bool OrderAttributesByName { get; set; }

        // Element formatting

        [Category("Element Formatting")]
        [DisplayName("Put Ending Brackets On New Line")]
        [JsonProperty("PutEndingBracketOnNewLine", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [Description("Defines whether to put ending brackets on a new line.\r\n\r\nDefault Value: false")]
        [DefaultValue(false)]
        public bool PutEndingBracketOnNewLine { get; set; }

        [Category("Element Formatting")]
        [DisplayName("Remove Ending Tag of Empty Elements")]
        [JsonProperty("RemoveEndingTagOfEmptyElement", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [Description("Defines whether to remove the end tag of an empty element.\r\n\r\nDefault Value: true")]
        [DefaultValue(true)]
        public bool RemoveEndingTagOfEmptyElement { get; set; }

        [Category("Element Formatting")]
        [DisplayName("Space Before Ending Slash in Self-Closing Elements")]
        [JsonProperty("SpaceBeforeClosingSlash", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [Description("Defines whether there should be a space before the slash in ending brackets for self-closing elements.\r\n\r\nDefault Value: true")]
        [DefaultValue(true)]
        public bool SpaceBeforeClosingSlash { get; set; }

        [Category("Element Formatting")]
        [DisplayName("Root element line breaks between attributes")]
        [JsonProperty("RootElementLineBreakRule", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [Description("Defines whether attributes of the document root element are broken into multiple lines.\r\n\r\nDefault Value: Default (use same rules as other elements)")]
        [DefaultValue(LineBreakRule.Default)]
        public LineBreakRule RootElementLineBreakRule { get; set; }

        // Element reordering

        [Category("Element Reordering")]
        [DisplayName("Reorder Visual State Manager")]
        [JsonProperty("ReorderVSM", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [Description("Defines whether to reorder the visual state manager. When set to first or last, the visual state manager will be moved to the first or last child element in its parent, respectively, otherwise it will not be moved.\r\n\r\nDefault Value: Last")]
        [DefaultValue(VisualStateManagerRule.Last)]
        public VisualStateManagerRule ReorderVSM { get; set; }

        [Category("Element Reordering")]
        [DisplayName("Reorder Grid Panel Children")]
        [JsonProperty("ReorderGridChildren", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [Description("Defines whether to reorder the children of a Grid by row/column. When true, children will be reordered in an ascending fashion by looking first at Grid.Row, then by Grid.Column.\r\n\r\nDefault Value: false")]
        [DefaultValue(false)]
        public bool ReorderGridChildren { get; set; }

        [Category("Element Reordering")]
        [DisplayName("Reorder Canvas Panel Children")]
        [JsonProperty("ReorderCanvasChildren", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [Description("Defines whether to reorder the children of a Canvas by left/top/right/bottom. When true, children will be reordered in an ascending fashion by first at Canvas.Left, then by Canvas.Top, Canvas.Right, and finally, Canvas.Bottom.\r\n\r\nDefault Value: false")]
        [DefaultValue(false)]
        public bool ReorderCanvasChildren { get; set; }

        [Category("Element Reordering")]
        [DisplayName("Reorder Setters")]
        [JsonProperty("ReorderSetters", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [Description("Defines whether to reorder 'Setter' elements in style/trigger elements. When this is set, children will be reordered in an ascending fashion by looking at their Property and/or TargetName properties.\r\n\r\nDefault Value: None")]
        [DefaultValue(ReorderSettersBy.None)]
        public ReorderSettersBy ReorderSetters { get; set; }

        //Markup Extension Handling

        [Category("Markup Extension Handling")]
        [DisplayName("Enable markup extension formatting")]
        [JsonProperty("FormatMarkupExtension", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [Description("Defines whether to format Markup Extensions (attributes containing '{}'). When true, attributes with markup extensions will always be put on a new line, unless the element is under the attribute tolerance or one of the specified elements is in the list of elements with no line breaks between attributes.\r\n\r\nDefault Value: true")]
        [DefaultValue(true)]
        public bool FormatMarkupExtension { get; set; }

        [Category("Markup Extension Handling")]
        [DisplayName("Keep markup extensions of these types on one line")]
        [JsonProperty("NoNewLineMarkupExtensions", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [Description("Defines a comma-separated list of Markup Extensions that are always kept on a single line\r\n\r\nDefault Value: x:Bind, Binding")]
        [DefaultValue("x:Bind, Binding")]
        public string NoNewLineMarkupExtensions { get; set; }

        // Thickness formatting

        [Category("Thickness formatting")]
        [DisplayName("Thickness Separator")]
        [JsonProperty("ThicknessSeparator", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [Description("Defines how thickness attributes (i.e., margin, padding, etc.) should be formatted.\r\n\r\nDefault Value: Comma")]
        [DefaultValue(ThicknessSeparator.Comma)]
        public ThicknessSeparator ThicknessSeparator { get; set; }

        [Category("Thickness formatting")]
        [DisplayName("Thickness Attributes")]
        [JsonProperty("ThicknessAttributes", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [Description("Defines a list of attributes that get reformatted if content appears to be a thickness.\r\n\r\nDefault Value: Margin, Padding, BorderThickness, ThumbnailClipMargin")]
        [DefaultValue("Margin, Padding, BorderThickness, ThumbnailClipMargin")]
        public string ThicknessAttributes { get; set; }

        // Misc

        [Category("Miscellaneous")]
        [DisplayName("Format XAML on Save")]
        [JsonProperty("FormatOnSave", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [Description("Defines whether to automatically format the active XAML document while saving.\r\n\r\nDefault Value: false")]
        [DefaultValue(false)]
        public bool FormatOnSave { get; set; }

        [Category("Miscellaneous")]
        [DisplayName("Comment Padding")]
        [JsonProperty("CommentPadding", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [Description("Determines the number of spaces a XAML comment should be padded with.\r\n\r\nDefault Value: 2")]
        [DefaultValue(2)]
        public int CommentPadding { get; set; }

        // Configuration

        private bool resetToDefault;

        [Category("XAML Magic Configuration")]
        [RefreshProperties(RefreshProperties.All)]
        [DisplayName("Reset to Default")]
        [Description("When set to true, all XAML Magic settings will be reset to their defaults.\r\n\r\nDefault Value: false")]
        [DefaultValue(false)]
        [JsonIgnore]
        public bool ResetToDefault
        {
            get { return this.resetToDefault; }
            set
            {
                this.resetToDefault = value;

                if (this.resetToDefault)
                {
                    this.ConfigPath = String.Empty;
                    this.InitializeProperties();
                }
            }
        }

        private string configPath = String.Empty;

        [Category("XAML Magic Configuration")]
        [RefreshProperties(RefreshProperties.All)]
        [DisplayName("External Configuration File")]
        [Description("Defines location of external XAML Magic configuration file. Specifying an external configuration file allows you to easily point multiple instances to a shared configuration. The configuration path can be local or network-based. Invalid configurations will be ignored.\r\n\r\nDefault Value: N/A")]
        [DefaultValue("")]
        [JsonIgnore]
        public string ConfigPath
        {
            get { return this.configPath; }
            set
            {
                this.configPath = value;
                this.TryLoadExternalConfiguration();
            }
        }

        private void InitializeProperties()
        {
            if (!this.TryLoadExternalConfiguration())
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream(StylerOptions.DefaultOptionsPath))
                using (StreamReader reader = new StreamReader(stream))
                {
                    this.LoadConfiguration(reader.ReadToEnd());
                }
            }
        }

        private bool TryLoadExternalConfiguration()
        {
            if (String.IsNullOrWhiteSpace(this.ConfigPath) || !File.Exists(this.ConfigPath))
            {
                return false;
            }

            return this.LoadConfiguration(File.ReadAllText(this.ConfigPath));
        }

        private bool LoadConfiguration(string config)
        {
            try
            {
                StylerOptions configOptions = JsonConvert.DeserializeObject<StylerOptions>(config);

                if (configOptions == null)
                {
                    this.LoadFallbackConfiguration();
                }
                else
                {
                    foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(this))
                    {
                        if (propertyDescriptor.Name.Equals(nameof(this.SerializedAttributeOrderingRuleGroups))
                            || propertyDescriptor.Name.Equals(nameof(this.ConfigPath)))
                        {
                            continue;
                        }

                        propertyDescriptor.SetValue(this, propertyDescriptor.GetValue(configOptions));
                    }

                    if (this.AttributeOrderingRuleGroups == null)
                    {
                        this.AttributeOrderingRuleGroups = this.DefaultAttributeOrderingRuleGroups;
                    }
                }
            }
            catch (Exception)
            {
                this.LoadFallbackConfiguration();
                return false;
            }

            return true;
        }

        private void LoadFallbackConfiguration()
        {
            // Initialize all properties with "DefaultValueAttrbute" to their default value
            foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(this))
            {
                // Set default value if DefaultValueAttribute is present
                DefaultValueAttribute attribute
                    = propertyDescriptor.Attributes[typeof(DefaultValueAttribute)] as DefaultValueAttribute;

                if (attribute != null)
                {
                    propertyDescriptor.SetValue(this, attribute.Value);
                }
            }

            this.AttributeOrderingRuleGroups = this.DefaultAttributeOrderingRuleGroups;
        }
    }
}