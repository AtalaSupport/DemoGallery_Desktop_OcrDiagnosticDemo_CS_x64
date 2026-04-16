using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Reflection;
using System.Collections.Specialized;
using System.IO;
using Atalasoft.Imaging;
using Atalasoft.Imaging.WinControls;
using Atalasoft.Ocr;
using Atalasoft.Ocr.GlyphReader;
using Atalasoft.Ocr.Tesseract;
using System.Globalization;
using Atalasoft.Imaging.Codec;
using Atalasoft.Ocr.OmniPage;


namespace OcrDiagnostic
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		protected enum InfoLocation 
		{
			ul, ur, bl, br
		};

		protected class ClickableItem 
		{
			private Rectangle _bounds;
			private object _thing;
			public ClickableItem(Rectangle theBounds, object theThing)
			{
				_bounds = theBounds;
				_thing = theThing;
			}
			public Rectangle Bounds { get { return _bounds; } }
			public object Thing { get { return _thing; } }
		}

		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem File;
		private System.Windows.Forms.MenuItem Recognize;
		private System.Windows.Forms.MenuItem Exit;
		private System.Windows.Forms.MenuItem Options;
		private System.Windows.Forms.MenuItem View;
		private System.Windows.Forms.MenuItem AutoRotate;
		private System.Windows.Forms.MenuItem Deskew;
		private System.Windows.Forms.MenuItem Despeckle;
		private System.Windows.Forms.MenuItem Flip;
		private System.Windows.Forms.MenuItem Invert;
        private System.Windows.Forms.MenuItem ToBilevel;
        private IContainer components;

		private GlyphReaderEngine _glyphReaderEngine;
        private OmniPageLoader _omniPageLoader;
        private OmniPageEngine _omniPageEngine;
        private Tesseract3Engine _tesseract3Engine;  // added for tesseract3 support
        private Tesseract5Engine _tesseract5Engine;  // added for tesseract5 support
        private OcrEngine _engine;                      // current active engine, if any

		private System.Windows.Forms.Panel OcrPane;
		private OcrDocument _theDoc = null;
		private Icon QuestionUp, QuestionDown;
		private Pen PicturePen, WordBoundingBoxPen, BaselinePen, GlobalBaselinePen, LineBoundingBoxPen,
			GlyphBoundingBoxPen;
		private ResolutionFontBuilder builder;
		private ArrayList _clickables;
		private ClickableItem _clicked;
		private System.Windows.Forms.MenuItem ShowWordBaselines;
		private System.Windows.Forms.MenuItem ShowLineBoundingBoxes;
		private System.Windows.Forms.MenuItem ShowWordBoundingBoxes;
		private System.Windows.Forms.MenuItem ShowGlyphBoundingBoxes;
		private System.Windows.Forms.MenuItem ShowLineBaselines;
		private System.Windows.Forms.MenuItem ShowFontNames;
		private bool _wasInClicked;
		private Font FontNameFont;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem AboutButton;
        private System.Windows.Forms.MenuItem menuItem2;
        private MenuItem menuGlyphReader;
        private MenuItem menuOmniPage;
		private System.Windows.Forms.MenuItem menuLanguage;
		private System.Windows.Forms.StatusBar statusBar1;
		private System.Windows.Forms.ProgressBar progressBar;
		private Brush FontBrush;
        private bool _validLicense;
        private bool _hasEV = false;
        private bool _hasGR = false;
        private MenuItem menuTesseract3;
        private MenuItem menuTesseract5;
        private bool _hasOmniPage = false;

		public Form1()
		{
			CheckLicenseFile();

            AtalaDemos.HelperMethods.PopulateDecoders(RegisteredDecoders.Decoders);

			if (this._validLicense)
			{
				//
				// Required for Windows Form Designer support
				//
				InitializeComponent();


				QuestionUp = new Icon(this.GetType(), "qmarkup.ico");
				QuestionDown = new Icon(this.GetType(), "qmarkdown.ico");
				PicturePen = new Pen(Color.MediumVioletRed);
				PicturePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
				WordBoundingBoxPen = new Pen(Color.IndianRed);
				LineBoundingBoxPen = new Pen(Color.ForestGreen);
				GlyphBoundingBoxPen = new Pen(Color.MediumBlue);
				BaselinePen = new Pen(Color.DodgerBlue);
				BaselinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
				GlobalBaselinePen = new Pen(Color.SeaGreen);
				GlobalBaselinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDotDot;
				builder = new ResolutionFontBuilder();
				_clickables = new ArrayList();
	
				ShowWordBaselines.Checked = true;
				ShowLineBoundingBoxes.Checked = true;
				ShowWordBoundingBoxes.Checked = true;
				ShowGlyphBoundingBoxes.Checked = true;
				ShowLineBaselines.Checked = true;
				ShowFontNames.Checked = true;
				FontNameFont = new Font("Verdana", 6);
				FontBrush = Brushes.LightBlue;

                // Pick a licensed engine to start with.
                menuOmniPage.Enabled = _hasOmniPage;
                if (_engine == null && _hasOmniPage)
                {
                    if (_omniPageLoader == null)
                    {
                        // PLEASE NOTE: this is the default location for when using our SDK
                        // you will need to ensure when using OmniPageEngine in yhour application that you 
                        // specify the resources directory wher the OmniPageResources live
                        // Please see https://www.atalasoft.com/KB2/KB/50396/INFO-OmniPageEngine-Overview
                        string omniPageOcrResourcesDirectory = @"C:\Program Files (x86)\Atalasoft\DotImage 11.3\bin\OCRResources\OmniPage";
                        if (!Directory.Exists(omniPageOcrResourcesDirectory))
                        {
                            MessageBox.Show("You need to ensure you ahve downloaded the OmniPageEngine OCR Resources to the OmniPage resource directory\r\nsee https://www.atalasoft.com/KB2/KB/50396/INFO-OmniPageEngine-Overview");
                        }
                        _omniPageLoader = new OmniPageLoader(omniPageOcrResourcesDirectory);
                    }
                    _omniPageEngine = new OmniPageEngine();
                    SelectEngine(_omniPageEngine);
                }

                this.menuGlyphReader.Enabled = _hasGR;
                if (_engine == null && _hasGR)
                {
                    _glyphReaderEngine = new GlyphReaderEngine();
                    _glyphReaderEngine.DefaultFontName = "Times New Roman";
                    SelectEngine(_glyphReaderEngine);
                }

                this.menuTesseract5.Enabled = true;
                if (_engine == null)
                {
                    _tesseract5Engine = new Tesseract5Engine();
                    SelectEngine(_tesseract5Engine);
                }

            }
		}

		#region Check for license code

		private void CheckGRLicense()
		{
			try
			{
				_hasGR = true;
				GlyphReaderEngine gr = new GlyphReaderEngine(); // does not throw
				gr.Initialize(); // will throw on no license
				gr.Dispose();
			}
			catch(AtalasoftLicenseException)
			{
				_hasGR = false;
			}
		}

        private void CheckOmniPageLicense()
        {
            try
            {
                _hasOmniPage = true;
                // PLEASE NOTE: this is the default location for when using our SDK
                // you will need to ensure when using OmniPageEngine in yhour application that you 
                // specify the resources directory wher the OmniPageResources live
                // Please see https://www.atalasoft.com/KB2/KB/50396/INFO-OmniPageEngine-Overview 
                string omniPageOcrResourcesDirectory = @"C:\Program Files (x86)\Atalasoft\DotImage 11.3\bin\OCRResources\OmniPage";
                if (!Directory.Exists(omniPageOcrResourcesDirectory))
                {
                    MessageBox.Show("You need to ensure you ahve downloaded the OmniPageEngine OCR Resources to the OmniPage resource directory\r\nsee https://www.atalasoft.com/KB2/KB/50396/INFO-OmniPageEngine-Overview");
                }
                _omniPageLoader = new OmniPageLoader(omniPageOcrResourcesDirectory);
                _omniPageEngine = new OmniPageEngine();
                _omniPageEngine.Initialize();
                _omniPageEngine.ShutDown();
            }
            catch (AtalasoftLicenseException)
            {
                _hasOmniPage = false;
                _omniPageEngine = null;
            }
            catch (FileNotFoundException)
            {
                _hasOmniPage = false;
                _omniPageEngine = null;
                MessageBox.Show("OmniPage Resources not found\n\nIf you'd like to use OmniPage, please see https://www.atalasoft.com/KB2/KB/50396/INFO-OmniPageEngine-Overview");
            }
        }

		private void CheckLicenseFile()
		{
			// Make sure a license for DotImage and Advanced DocClean exist.
			try
			{
				AtalaImage img = new AtalaImage();
				img.Dispose();
			}
			catch (Atalasoft.Imaging.AtalasoftLicenseException)
			{
				LicenseCheckFailure("This demo requires a DotImage license and an OCR license.");
				return;
			}
			
			if (AtalaImage.Edition != LicenseEdition.Document)
			{
				LicenseCheckFailure("This demo requires a Document Imaging License.\r\nYour current license is for '" + AtalaImage.Edition.ToString() + "'.");
				return;
			}

			try
			{
				TranslatorCollection t = new TranslatorCollection();
			}
			catch(AtalasoftLicenseException)
			{
				LicenseCheckFailure("This demo requires an OCR license.");
				return;
			}

			CheckGRLicense();
            CheckOmniPageLicense();

			if (_hasEV || _hasGR || _hasOmniPage)							
			{
				this._validLicense = true;
			}
			else			
				LicenseCheckFailure("No OCR Engine is licensed on your system.  Please request an evaluation license for one of these engines before running this demo.");
		}

		private void LicenseCheckFailure(string message)
		{
			this.Load += new System.EventHandler(this.Form1_Load);
			if (MessageBox.Show(this, message + "\r\n\r\nWould you like to request an evaluation license?", "License Required", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
			{
				// Locate the activation utility.
				string path = "";
				Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software\Atalasoft\dotImage\9.0");
				if (key != null)
				{
					path = Convert.ToString(key.GetValue("AssemblyBasePath"));
					if (path != null && path.Length > 5)
						path = path.Substring(0, path.Length - 3) + "AtalasoftToolkitActivation.exe";
					else
						path = Path.GetFullPath(@"..\..\..\AtalasoftToolkitActivation.exe");

					key.Close();
				}

				if (System.IO.File.Exists(path))
					System.Diagnostics.Process.Start(path);
				else
					MessageBox.Show(this, "We were unable to location the DotImage activation utility.\r\nPlease run it from the Start menu shortcut.", "File Not Found");
			}
		}

		private void Form1_Load(object sender, System.EventArgs e)
		{
			// close the demo if there is no valid license
			if (!this._validLicense)
				Application.Exit();
		}

		#endregion

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (_engine != null) _engine.ShutDown();
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.File = new System.Windows.Forms.MenuItem();
            this.Recognize = new System.Windows.Forms.MenuItem();
            this.Exit = new System.Windows.Forms.MenuItem();
            this.View = new System.Windows.Forms.MenuItem();
            this.ShowWordBaselines = new System.Windows.Forms.MenuItem();
            this.ShowLineBaselines = new System.Windows.Forms.MenuItem();
            this.ShowLineBoundingBoxes = new System.Windows.Forms.MenuItem();
            this.ShowWordBoundingBoxes = new System.Windows.Forms.MenuItem();
            this.ShowGlyphBoundingBoxes = new System.Windows.Forms.MenuItem();
            this.ShowFontNames = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuGlyphReader = new System.Windows.Forms.MenuItem();
            this.menuOmniPage = new System.Windows.Forms.MenuItem();
            this.menuTesseract3 = new System.Windows.Forms.MenuItem();
            this.Options = new System.Windows.Forms.MenuItem();
            this.AutoRotate = new System.Windows.Forms.MenuItem();
            this.Deskew = new System.Windows.Forms.MenuItem();
            this.Despeckle = new System.Windows.Forms.MenuItem();
            this.Flip = new System.Windows.Forms.MenuItem();
            this.Invert = new System.Windows.Forms.MenuItem();
            this.ToBilevel = new System.Windows.Forms.MenuItem();
            this.menuLanguage = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.AboutButton = new System.Windows.Forms.MenuItem();
            this.OcrPane = new System.Windows.Forms.Panel();
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.menuTesseract5 = new System.Windows.Forms.MenuItem();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.File,
            this.View,
            this.menuItem2,
            this.Options,
            this.menuLanguage,
            this.menuItem1});
            // 
            // File
            // 
            this.File.Index = 0;
            this.File.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.Recognize,
            this.Exit});
            this.File.Text = "File";
            // 
            // Recognize
            // 
            this.Recognize.Index = 0;
            this.Recognize.Text = "Recognize...";
            this.Recognize.Click += new System.EventHandler(this.Recognize_Click);
            // 
            // Exit
            // 
            this.Exit.Index = 1;
            this.Exit.Text = "Exit";
            this.Exit.Click += new System.EventHandler(this.Exit_Click);
            // 
            // View
            // 
            this.View.Index = 1;
            this.View.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.ShowWordBaselines,
            this.ShowLineBaselines,
            this.ShowLineBoundingBoxes,
            this.ShowWordBoundingBoxes,
            this.ShowGlyphBoundingBoxes,
            this.ShowFontNames});
            this.View.Text = "View";
            // 
            // ShowWordBaselines
            // 
            this.ShowWordBaselines.Index = 0;
            this.ShowWordBaselines.Text = "Word Baselines";
            this.ShowWordBaselines.Click += new System.EventHandler(this.ShowWordBaselines_Click);
            // 
            // ShowLineBaselines
            // 
            this.ShowLineBaselines.Index = 1;
            this.ShowLineBaselines.Text = "Line Baselines";
            this.ShowLineBaselines.Click += new System.EventHandler(this.ShowLineBaselines_Click);
            // 
            // ShowLineBoundingBoxes
            // 
            this.ShowLineBoundingBoxes.Index = 2;
            this.ShowLineBoundingBoxes.Text = "Line Bounding Boxes";
            this.ShowLineBoundingBoxes.Click += new System.EventHandler(this.ShowLineBoundingBoxes_Click);
            // 
            // ShowWordBoundingBoxes
            // 
            this.ShowWordBoundingBoxes.Index = 3;
            this.ShowWordBoundingBoxes.Text = "Word Bounding Boxes";
            this.ShowWordBoundingBoxes.Click += new System.EventHandler(this.ShowWordBoundingBoxes_Click);
            // 
            // ShowGlyphBoundingBoxes
            // 
            this.ShowGlyphBoundingBoxes.Index = 4;
            this.ShowGlyphBoundingBoxes.Text = "Glyph Bounding Boxes";
            this.ShowGlyphBoundingBoxes.Click += new System.EventHandler(this.ShowGlyphBoundingBoxes_Click);
            // 
            // ShowFontNames
            // 
            this.ShowFontNames.Index = 5;
            this.ShowFontNames.Text = "Font Names";
            this.ShowFontNames.Click += new System.EventHandler(this.ShowFontNames_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 2;
            this.menuItem2.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuGlyphReader,
            this.menuOmniPage,
            this.menuTesseract3,
            this.menuTesseract5});
            this.menuItem2.Text = "Engine";
            this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
            // 
            // menuGlyphReader
            // 
            this.menuGlyphReader.Index = 0;
            this.menuGlyphReader.Text = "GlyphReader";
            this.menuGlyphReader.Click += new System.EventHandler(this.menuGlyphReader_Click);
            // 
            // menuOmniPage
            // 
            this.menuOmniPage.Index = 1;
            this.menuOmniPage.Text = "OmniPage";
            this.menuOmniPage.Click += new System.EventHandler(this.menuOmniPage_Click);
            // 
            // menuTesseract3
            // 
            this.menuTesseract3.Index = 2;
            this.menuTesseract3.Text = "Tesseract 3";
            this.menuTesseract3.Click += new System.EventHandler(this.menuTesseract3_Click);
            // 
            // Options
            // 
            this.Options.Index = 3;
            this.Options.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.AutoRotate,
            this.Deskew,
            this.Despeckle,
            this.Flip,
            this.Invert,
            this.ToBilevel});
            this.Options.Text = "Options";
            // 
            // AutoRotate
            // 
            this.AutoRotate.Index = 0;
            this.AutoRotate.Text = "Auto Rotate";
            this.AutoRotate.Click += new System.EventHandler(this.AutoRotate_Click);
            // 
            // Deskew
            // 
            this.Deskew.Index = 1;
            this.Deskew.Text = "Deskew";
            this.Deskew.Click += new System.EventHandler(this.Deskew_Click);
            // 
            // Despeckle
            // 
            this.Despeckle.Index = 2;
            this.Despeckle.Text = "Despeckle";
            this.Despeckle.Click += new System.EventHandler(this.Despeckle_Click);
            // 
            // Flip
            // 
            this.Flip.Index = 3;
            this.Flip.Text = "Flip Left/Right";
            this.Flip.Click += new System.EventHandler(this.Flip_Click);
            // 
            // Invert
            // 
            this.Invert.Index = 4;
            this.Invert.Text = "Invert";
            this.Invert.Click += new System.EventHandler(this.Invert_Click);
            // 
            // ToBilevel
            // 
            this.ToBilevel.Index = 5;
            this.ToBilevel.Text = "Convert to Bilevel";
            this.ToBilevel.Click += new System.EventHandler(this.Binarize_Click);
            // 
            // menuLanguage
            // 
            this.menuLanguage.Index = 4;
            this.menuLanguage.Text = "Language";
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 5;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.AboutButton});
            this.menuItem1.Text = "Help";
            // 
            // AboutButton
            // 
            this.AboutButton.Index = 0;
            this.AboutButton.Text = "About ...";
            this.AboutButton.Click += new System.EventHandler(this.AboutButton_Click);
            // 
            // OcrPane
            // 
            this.OcrPane.AutoScroll = true;
            this.OcrPane.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OcrPane.Location = new System.Drawing.Point(0, 0);
            this.OcrPane.Name = "OcrPane";
            this.OcrPane.Size = new System.Drawing.Size(608, 558);
            this.OcrPane.TabIndex = 0;
            this.OcrPane.Paint += new System.Windows.Forms.PaintEventHandler(this.OcrPane_Paint);
            this.OcrPane.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OcrPane_MouseDown);
            this.OcrPane.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OcrPane_MouseMove);
            this.OcrPane.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OcrPane_MouseUp);
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 558);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Size = new System.Drawing.Size(608, 27);
            this.statusBar1.TabIndex = 1;
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(247, 566);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(340, 14);
            this.progressBar.TabIndex = 2;
            this.progressBar.Visible = false;
            // 
            // menuTesseract5
            // 
            this.menuTesseract5.Index = 3;
            this.menuTesseract5.Text = "Tesseract5";
            this.menuTesseract5.Click += new System.EventHandler(this.menuTesseract5_Click);
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(608, 585);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.OcrPane);
            this.Controls.Add(this.statusBar1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Menu = this.mainMenu1;
            this.Name = "Form1";
            this.Text = "Ocr Diagnostic";
            this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			//OcrResourceLoader loader = new OcrResourceLoader();
            GlyphReaderLoader loader = new GlyphReaderLoader();

			Application.Run(new Form1());
		}

		private void Exit_Click(object sender, System.EventArgs e)
		{
			Application.Exit();
		}

		#region OptionMenu Handling

		private void MapNativeOptionsToMenus()
		{
            AutoRotate.Enabled = _engine.AvailablePreprocessingOptions.AutoRotate;
			AutoRotate.Checked = _engine.PreprocessingOptions.AutoRotate;
            Deskew.Enabled = _engine.AvailablePreprocessingOptions.Deskew;
			Deskew.Checked = _engine.PreprocessingOptions.Deskew;
            Despeckle.Enabled = _engine.AvailablePreprocessingOptions.Despeckle;
			Despeckle.Checked = _engine.PreprocessingOptions.Despeckle;
            Flip.Enabled = _engine.AvailablePreprocessingOptions.FlipLeftRight;
			Flip.Checked = _engine.PreprocessingOptions.FlipLeftRight;
            Invert.Enabled = _engine.AvailablePreprocessingOptions.Invert;
			Invert.Checked = _engine.PreprocessingOptions.Invert;
            ToBilevel.Enabled = _engine.AvailablePreprocessingOptions.ToBilevel;
			ToBilevel.Checked = _engine.PreprocessingOptions.ToBilevel;
		}

		private void AutoRotate_Click(object sender, System.EventArgs e)
		{
			_engine.PreprocessingOptions.AutoRotate = !_engine.PreprocessingOptions.AutoRotate;
			AutoRotate.Checked = _engine.PreprocessingOptions.AutoRotate;
		}

		private void Deskew_Click(object sender, System.EventArgs e)
		{
			_engine.PreprocessingOptions.Deskew = !_engine.PreprocessingOptions.Deskew;
			Deskew.Checked = _engine.PreprocessingOptions.Deskew;
		}

		private void Despeckle_Click(object sender, System.EventArgs e)
		{
			_engine.PreprocessingOptions.Despeckle = !_engine.PreprocessingOptions.Despeckle;
			Despeckle.Checked = _engine.PreprocessingOptions.Despeckle;
		}

		private void Flip_Click(object sender, System.EventArgs e)
		{
			_engine.PreprocessingOptions.FlipLeftRight = !_engine.PreprocessingOptions.FlipLeftRight;
			Flip.Checked = _engine.PreprocessingOptions.FlipLeftRight;
		}

		private void Invert_Click(object sender, System.EventArgs e)
		{
			_engine.PreprocessingOptions.Invert = !_engine.PreprocessingOptions.Invert;
			Invert.Checked = _engine.PreprocessingOptions.Invert;
		}

		private void Binarize_Click(object sender, System.EventArgs e)
		{
			_engine.PreprocessingOptions.ToBilevel = !_engine.PreprocessingOptions.ToBilevel;
			ToBilevel.Checked = _engine.PreprocessingOptions.ToBilevel;
		}
		#endregion

		private void Recognize_Click(object sender, System.EventArgs e)
		{
			OpenImageFileDialog oif = new OpenImageFileDialog();

			// try to locate images folder
			string imagesFolder = Application.ExecutablePath;
			// we assume we are running under the DotImage install folder
			int pos = imagesFolder.IndexOf("DotImage ");
			if (pos != -1)
			{
				imagesFolder = imagesFolder.Substring(0,imagesFolder.IndexOf(@"\",pos)) + @"\Images\OCR";
			}

			//use this folder as starting point			
			oif.InitialDirectory = imagesFolder;
            oif.Title = "Select File to OCR";
            oif.Filter = AtalaDemos.HelperMethods.CreateDialogFilter(true);

			if (oif.ShowDialog(this) != DialogResult.OK) 
			{
				return;
			}

			string[] paths = new string[1];
			paths[0] = oif.FileName;
			oif.Dispose();
			FileSystemImageSource source = new FileSystemImageSource(paths, false);

			try 
			{
				_theDoc = _engine.Recognize(source);
			}
			catch (Exception err)
			{
				MessageBox.Show("Recognition Failed: " + err.Message);
				_theDoc = null;
			}
			finally 
			{
                progressBar.Visible = false;
				DocChanged();
			}
		}

		private void DocChanged()
		{
			Size theSize;
			if (_theDoc == null) 
			{
				theSize = new Size(OcrPane.Width, OcrPane.Height);
				OcrPane.AutoScrollMinSize = theSize;
				_clickables.Clear();
			}
			else 
			{
				OcrPage page = _theDoc.Pages[0];
				theSize = new Size(page.Width, page.Height);
				OcrPane.AutoScrollMinSize = theSize;
				BuildClickables(page);
				OcrPane.AutoScrollPosition = new Point(0, 0);
			}
			OcrPane.Invalidate();
		}

		private void BuildClickables(OcrPage page)
		{
			Rectangle r;
			_clickables.Clear();
			foreach (OcrRegion region in page.Regions) 
			{
				if (region is OcrTextRegion) 
				{
					OcrTextRegion textRegion = (OcrTextRegion)region;
					foreach(OcrLine line in textRegion.Lines) 
					{
						if (this.ShowLineBoundingBoxes.Checked) 
						{
							r = GetInfoBounds(line.Bounds, InfoLocation.bl, QuestionUp);
							_clickables.Add(new ClickableItem(r, line));
						}
						foreach(OcrWord word in line.Words) 
						{
							if (this.ShowWordBoundingBoxes.Checked) 
							{
								r = GetInfoBounds(word.Bounds, InfoLocation.ul, QuestionUp);
								_clickables.Add(new ClickableItem(r, word));
							}
							if (this.ShowGlyphBoundingBoxes.Checked) 
							{
								foreach (OcrGlyph glyph in word.Glyphs) 
								{
									r = GetInfoBounds(glyph.Bounds, InfoLocation.ur, QuestionUp);
									_clickables.Add(new ClickableItem(r, glyph));
								}
							}
						}
					}
				}
				else if (region is OcrImageRegion) 
				{
					r = GetInfoBounds(region.Bounds, InfoLocation.ul, QuestionUp);
					_clickables.Add(new ClickableItem(r, region));
				}
			}
		}

		private void OcrPane_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			if (_theDoc == null)
			{
				Rectangle clipRect = e.ClipRectangle;
				g.FillRectangle(Brushes.AliceBlue, clipRect);
			}
			else 
			{
				OcrPage page = _theDoc.Pages[0];

				Rectangle clipRect = e.ClipRectangle;
				g.FillRectangle(Brushes.White, clipRect);

				clipRect.X -= OcrPane.AutoScrollPosition.X;
				clipRect.Y -= OcrPane.AutoScrollPosition.Y;

				g.TranslateTransform(OcrPane.AutoScrollPosition.X, OcrPane.AutoScrollPosition.Y);

				foreach (OcrRegion region in page.Regions) 
				{
					if (region is OcrTextRegion) 
					{
						OcrTextRegion textRegion = (OcrTextRegion)region;
						foreach (OcrLine line in textRegion.Lines) 
						{
							DrawLine(g, line, page.Resolution);
						}
					}
					else if (region is OcrImageRegion) 
					{
						Rectangle destRect = region.Bounds;
						destRect.Offset(OcrPane.AutoScrollPosition.X, OcrPane.AutoScrollPosition.Y);
						OcrImageRegion imageRegion = (OcrImageRegion)region;
						imageRegion.Image.Draw(g, destRect);
						g.DrawRectangle(PicturePen, imageRegion.Bounds);
						DrawInfoIcon(g, imageRegion.Bounds, InfoLocation.ul, QuestionUp);
					}
				}
			}
		}

		private Rectangle GetInfoBounds(Rectangle bounds, InfoLocation loc, Icon icon)
		{
			int x, y;
			switch (loc) 
			{
				case InfoLocation.bl:
					x = bounds.Left;
					y = bounds.Bottom - icon.Height;
					break;
				case InfoLocation.br:
					x = bounds.Right - icon.Width;
					y = bounds.Bottom - icon.Height;
					break;
				case InfoLocation.ul:
					x = bounds.Left;
					y = bounds.Top;
					break;
				default:
				case InfoLocation.ur:
					x = bounds.Right - icon.Width;
					y = bounds.Top;
					break;
			}
			return new Rectangle(x, y, icon.Width, icon.Height);
		}

		private void DrawInfoIcon(Graphics g, Rectangle bounds, Icon icon)
		{
			g.DrawIcon(icon, bounds);
		}
		
		private void DrawInfoIcon(Graphics g, Rectangle bounds, InfoLocation loc, Icon icon)
		{
			Rectangle r = GetInfoBounds(bounds, loc, icon);
			g.DrawIcon(icon, r);
		}

		private void DrawLine(Graphics g, OcrLine line, Dpi imageResolution)
		{
			builder.Scale = imageResolution.X / g.DpiX;
			IFontMapper mapper = _engine.FontMapper;
			string text = line.Text;
			int minX, maxX;
			minX = -1;
			maxX = 0;
			foreach (OcrWord word in line.Words) 
			{
				if ( word.Glyphs.Count > 0 ) 
				{
					Rectangle bounds = line.Bounds;
					if (word.StyleIsUniform(mapper, builder)) 
					{
						double confidence = word.Confidence;
						int level = (int)(255.0 * (1.0 - (confidence * confidence)));
						Brush color = new SolidBrush(Color.FromArgb(level, level, level));
						Font font = word.GetFontAt(imageResolution, mapper, builder, 0);
						float size = font.Size;
						FontStyle style = font.Style;
						FontFamily family = word.GetFontFamilyAt(mapper, 0);
						float emheight = family.GetEmHeight(style);
						float conversionToPixels = size / emheight;
						float spacing = family.GetLineSpacing(style);
						int correction = (int)(spacing * conversionToPixels);

						g.DrawString(word.Text, font, color, word.Bounds.X, line.Baseline-correction, StringFormat.GenericTypographic);
						if (this.ShowWordBoundingBoxes.Checked) 
						{
							g.DrawRectangle(WordBoundingBoxPen, word.Bounds);
							DrawInfoIcon(g, word.Bounds, InfoLocation.ul, QuestionUp);
						}
						if (this.ShowWordBaselines.Checked) 
						{
							g.DrawLine(BaselinePen, word.Bounds.X, word.Baseline, word.Bounds.Right, word.Baseline);
						}
						color.Dispose();
						if (minX < 0) 
						{
							minX = word.Bounds.X;
						}
						else if (word.Bounds.X < minX) 
						{
							minX = word.Bounds.X;
						}
						if (word.Bounds.Right > maxX) 
						{
							maxX = word.Bounds.Right;
						}
					}
					if (ShowGlyphBoundingBoxes.Checked) 
					{
						foreach (OcrGlyph glyph in word.Glyphs) 
						{
							g.DrawRectangle(GlyphBoundingBoxPen, glyph.Bounds);
							DrawInfoIcon(g, glyph.Bounds, InfoLocation.ur, QuestionUp);
						}
					}
				}
			}
			if (this.ShowLineBaselines.Checked) 
			{
				g.DrawLine(GlobalBaselinePen, minX, line.Baseline, maxX, line.Baseline);
			}
			if (this.ShowLineBoundingBoxes.Checked) 
			{
				g.DrawRectangle(LineBoundingBoxPen, line.Bounds);
				DrawInfoIcon(g, line.Bounds, InfoLocation.bl, QuestionUp);
			}
			if (this.ShowFontNames.Checked) 
			{
				string fontName = line.GetFontNameAt(mapper, 0);
				SizeF size = g.MeasureString(fontName, FontNameFont);
				Rectangle r = new Rectangle(line.Bounds.Left + 18, line.Bounds.Bottom - ((int)size.Height) - 2,
					((int)size.Width) + 3, ((int)size.Height) + 2);
				g.FillRectangle(FontBrush, r);
				g.DrawRectangle(Pens.Black, r);
				g.DrawString(fontName, FontNameFont, Brushes.Black, (float)(r.X + 1), (float)(r.Y + 1));
			}
		}

		private void OcrPane_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			int X = e.X - OcrPane.AutoScrollPosition.X;
			int Y = e.Y - OcrPane.AutoScrollPosition.Y;
			_clicked = null;
			if (e.Button == MouseButtons.Left) 
			{
				foreach (object o in _clickables) 
				{
					ClickableItem item = (ClickableItem)o;
					if (item.Bounds.Contains(X, Y)) 
					{
						Graphics g = OcrPane.CreateGraphics();
						g.TranslateTransform(OcrPane.AutoScrollPosition.X, OcrPane.AutoScrollPosition.Y);
						DrawInfoIcon(g, item.Bounds, QuestionDown);
						g.Dispose();
						_clicked = item;
						_wasInClicked = true;
					}
				}
			}
		}

		private void OcrPane_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			int X = e.X - OcrPane.AutoScrollPosition.X;
			int Y = e.Y - OcrPane.AutoScrollPosition.Y;
			if (_clicked != null) 
			{
				if (_clicked.Bounds.Contains(X, Y)) 
				{
					Graphics g = OcrPane.CreateGraphics();
					g.TranslateTransform(OcrPane.AutoScrollPosition.X, OcrPane.AutoScrollPosition.Y);
					DrawInfoIcon(g, _clicked.Bounds, QuestionUp);
					g.Dispose();
					Object o = _clicked.Thing;
					DisplayInfo(o);
				}
				_clicked = null;
			}
		}

		private void DisplayInfo(Object o)
		{
			string typename = "unknown";
			if (o is OcrImageRegion) 
			{
				typename = "Image";
			}
			else if (o is OcrTextRegion) 
			{
				typename = "Text Region";
			}
			else if (o is OcrLine) 
			{
				typename = "Line";
			}
			else if (o is OcrWord) 
			{
				typename = "Word";
			}
			else if (o is OcrGlyph) 
			{
				typename = "Glyph";
			}
			Parameters myParameters = new Parameters(typename, o);
			myParameters.ShowDialog(this);
		}

		private void OcrPane_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			int X = e.X - OcrPane.AutoScrollPosition.X;
			int Y = e.Y - OcrPane.AutoScrollPosition.Y;
			if (_clicked != null) 
			{
				bool inClicked = _clicked.Bounds.Contains(X, Y);
				if (inClicked != _wasInClicked) 
				{
					Graphics g = OcrPane.CreateGraphics();
					g.TranslateTransform(OcrPane.AutoScrollPosition.X, OcrPane.AutoScrollPosition.Y);
					DrawInfoIcon(g, _clicked.Bounds, inClicked ? QuestionDown : QuestionUp);
					g.Dispose();
					_wasInClicked = inClicked;
				}
			}
		}

		private void ShowWordBaselines_Click(object sender, System.EventArgs e)
		{
			ShowWordBaselines.Checked = !ShowWordBaselines.Checked;
			OcrPane.Invalidate();
			if (_theDoc != null) 
			{
				BuildClickables(_theDoc.Pages[0]);
			}
		}

		private void ShowLineBaselines_Click(object sender, System.EventArgs e)
		{
			ShowLineBaselines.Checked = !ShowLineBaselines.Checked;
			OcrPane.Invalidate();
			if (_theDoc != null) 
			{
				BuildClickables(_theDoc.Pages[0]);
			}
		}

		private void ShowLineBoundingBoxes_Click(object sender, System.EventArgs e)
		{
			ShowLineBoundingBoxes.Checked = !ShowLineBoundingBoxes.Checked;
			OcrPane.Invalidate();
			if (_theDoc != null) 
			{
				BuildClickables(_theDoc.Pages[0]);
			}
		}

		private void ShowWordBoundingBoxes_Click(object sender, System.EventArgs e)
		{
			ShowWordBoundingBoxes.Checked = !ShowWordBoundingBoxes.Checked;
			OcrPane.Invalidate();
			if (_theDoc != null) 
			{
				BuildClickables(_theDoc.Pages[0]);
			}
		}

		private void ShowGlyphBoundingBoxes_Click(object sender, System.EventArgs e)
		{
			ShowGlyphBoundingBoxes.Checked = !ShowGlyphBoundingBoxes.Checked;
			OcrPane.Invalidate();
			if (_theDoc != null) 
			{
				BuildClickables(_theDoc.Pages[0]);
			}
		}

		private void ShowFontNames_Click(object sender, System.EventArgs e)
		{
			ShowFontNames.Checked = !ShowFontNames.Checked;
			OcrPane.Invalidate();
			if (_theDoc != null) 
			{
				BuildClickables(_theDoc.Pages[0]);
			}
		}

		private void AboutButton_Click(object sender, System.EventArgs e)
		{
			AtalaDemos.AboutBox.About aboutBox = new AtalaDemos.AboutBox.About("About Atalasoft DotImage OCR Diagnostic Demo",
				"DotImage OCR Diagnostic Demo");
			aboutBox.Description = @"The purpose of this demo is to show what the OCR engine recognizes in a document using the various engines supplied with DotImage OCR.  When translating an image, all the areas that are recognized are shown, along with the resulting text.  This program is useful for diagnosing OCR, comparing results with different engines, and to demonstrate some more advanced features using the OcrDocument class, which can be used to traverse a recognized page.  Requires DotImage, DotImage OCR, and a license for at least one OCR Engine.";
			aboutBox.ShowDialog();
		}

		private void SelectEngine(OcrEngine newEngine)
		{
            if (_engine != null)
            {
                _engine.ShutDown();
            }
			_engine = newEngine;
            if (_engine != null)
            {
                _engine.Initialize();
            }
            menuGlyphReader.Checked = (_engine == _glyphReaderEngine);
            menuOmniPage.Checked = (_engine == _omniPageEngine);
            menuTesseract3.Checked = (_engine == _tesseract3Engine);
            menuTesseract5.Checked = (_engine == _tesseract5Engine);
            
         
            MapNativeOptionsToMenus();
            CreateLanguageMenu();
            //hook in OCR events
            _engine.DocumentProgress += new OcrDocumentProgressEventHandler(_engine_DocumentProgress);
        }

		private void language_Click(object sender, EventArgs e)
		{
			MenuItem selecteditem = (MenuItem)sender;
			foreach (MenuItem item in this.menuLanguage.MenuItems)
			{
				item.Checked = (item==selecteditem);
			}
			CultureInfo[] cultures = _engine.GetSupportedRecognitionCultures();
			foreach (CultureInfo info in cultures)
			{
				if (info.DisplayName == selecteditem.Text)
					_engine.RecognitionCulture = info;
			}
		}

		private void CreateLanguageMenu()
		{
			// build language/culture menu
			CultureInfo[] cultures = _engine.GetSupportedRecognitionCultures();
            StringCollection names = new StringCollection();
            foreach (CultureInfo info in cultures)
            {
                names.Add(info.DisplayName);
            }
            // Sort into alphabetical order
            ArrayList.Adapter(names).Sort();
            // Create menu
			EventHandler ev = new EventHandler(language_Click);
			this.menuLanguage.MenuItems.Clear();
            foreach (String name in names)
			{
				MenuItem mi = new MenuItem(name, ev);
				this.menuLanguage.MenuItems.Add(mi);
				if (_engine.RecognitionCulture.DisplayName == name)
					mi.Checked = true;
			}
		}

        private void menuGlyphReader_Click(object sender, System.EventArgs e)
		{
            if (_glyphReaderEngine == null)
            {
                try
                {
                    _glyphReaderEngine = new GlyphReaderEngine();
                }
                catch (Exception err)
                {
                    MessageBox.Show(this, "Unable to Create GlyphReader Engine: " + err.Message);
                    _glyphReaderEngine = null;
                }
            }
            if (_glyphReaderEngine != null && _engine != _glyphReaderEngine)
            {
                SelectEngine(_glyphReaderEngine);
            }
		}


        private void menuOmniPage_Click(object sender, EventArgs e)
        {

            if (_omniPageEngine == null)
            {
                try
                {
                    _omniPageEngine = new OmniPageEngine();
                }
                catch (Exception err)
                {
                    MessageBox.Show(this, "Unable to Create OmniPage Engine: " + err.Message);
                    _omniPageEngine = null;
                }
            }
            if (_omniPageEngine != null && _engine != _omniPageEngine)
            {
                SelectEngine(_omniPageEngine);
            }
        }



        private void menuTesseract5_Click(object sender, EventArgs e)
        {
            if (_tesseract5Engine == null)
            {
                try
                {
                    _tesseract5Engine = new Tesseract5Engine();
                }
                catch (Exception err)
                {
                    MessageBox.Show(this, "Unable to Create Tesseract 3 Engine: " + err.Message);
                    _tesseract5Engine = null;
                }
            }
            if (_tesseract5Engine != null && _engine != _tesseract5Engine)
            {
                SelectEngine(_tesseract5Engine);
            }
        }

        private void menuTesseract3_Click(object sender, EventArgs e)
        {
            if (_tesseract3Engine == null)
            {
                try
                {
                    _tesseract3Engine = new Tesseract3Engine();
                }
                catch (Exception err)
                {
                    MessageBox.Show(this, "Unable to Create Tesseract 3 Engine: " + err.Message);
                    _tesseract3Engine = null;
                }
            }
            if (_tesseract3Engine != null && _engine != _tesseract3Engine)
            {
                SelectEngine(_tesseract3Engine);
            }
        }

        private void _engine_DocumentProgress(object sender, OcrDocumentProgressEventArgs e)
		{
			statusBar1.Text = EnglishStringFromOcrStage(e.Stage);
            progressBar.Visible = true;
			progressBar.Value = e.Progress;
			statusBar1.Refresh();
			progressBar.Refresh();
		}

		private string EnglishStringFromOcrStage(OcrDocumentStage stage)
		{
            String engineName = _engine.GetType().Name;
			switch (stage)
			{
				case OcrDocumentStage.BeginDocument:
					return engineName+": Recognizing Document...";
				case OcrDocumentStage.BeginPage:
					return engineName+": Recognizing Page...";
				case OcrDocumentStage.EndPage:
					return engineName+": End Recognizing Page...";
				case OcrDocumentStage.EndDocument:
                    progressBar.Visible = false;
					return engineName+": Done Recognizing";
				default:
					return "";

												
			}
		}

        private void menuItem2_Click(object sender, EventArgs e)
        {

        }



	}
}
