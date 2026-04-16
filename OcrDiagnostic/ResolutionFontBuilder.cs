using System;
using System.Drawing;
using System.Drawing.Text;
using Atalasoft.Ocr;

namespace OcrDiagnostic
{
	/// <summary>
	/// Summary description for ResolutionFontBuilder.
	/// </summary>
	public class ResolutionFontBuilder : BasicFontBuilder
	{
		double _scale;

		public ResolutionFontBuilder()
			: base()
		{
			_scale = 1.0;
		}

		public override Font BuildFont(FontFamily family, float size, FontStyle style)
		{
			size *= (float)_scale;
			return base.BuildFont(family, size, style);
		}

		public double Scale 
		{
			get { return _scale; } set { _scale = value; } 
		}
	}
}
