//--------------------------------------------------
// 6502bench Sanrio World Smash Ball! Font Visualizer
//   Created based on RuntimeData/... visualizer.
//   Follow the original license. (Apache 2.0)
//--------------------------------------------------

// Original license
/*
 * Copyright 2019 faddenSoft
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.ObjectModel;

using PluginCommon;

namespace RuntimeData.Swsb{
	public class VisFont : MarshalByRefObject, IPlugin, IPlugin_Visualizer{
		// IPlugin
		public string		Identifier{
			get{
				return "Swsb Font Visualizer";
			}
		}
		private IApplication	mAppRef;
		private byte[]		mFileData;

		// Visualization identifiers; DO NOT change or projects that use them will break.
		private const string	VIS_GEN_BITMAP	= "swsb-font";

		private const string	P_OFFSET	= "offset";
		private const string	P_FONTWIDTH	= "fontwidth";
		private const string	P_FONTHEIGHT	= "fontheight";
		private const string	P_TILEWIDTH	= "tilewidth";
		private const string	P_TILEHEIGHT	= "tileheight";
		private const string	P_COLOR_R	= "fontcolor-r";
		private const string	P_COLOR_G	= "fontcolor-g";
		private const string	P_COLOR_B	= "fontcolor-b";

		// Visualization descriptors.
		private VisDescr[]	mDescriptors	= new VisDescr[]{
			new VisDescr(VIS_GEN_BITMAP, "SWSB FONT", VisDescr.VisType.Bitmap,
				new VisParamDescr[]{
					new VisParamDescr("File offset (hex)",	P_OFFSET,	typeof(int),  0, 0x00FFFFFF, VisParamDescr.SpecialMode.Offset, 0),
					new VisParamDescr("Font width tile",	P_FONTWIDTH,	typeof(int),  1, 512, VisParamDescr.SpecialMode.None, 12),
					new VisParamDescr("Font height tile",	P_FONTHEIGHT,	typeof(int),  1, 512, VisParamDescr.SpecialMode.None, 12),
					new VisParamDescr("Tile width tile",	P_TILEWIDTH,	typeof(int),  1, 512, VisParamDescr.SpecialMode.None, 16),
					new VisParamDescr("Tile height tile",	P_TILEHEIGHT,	typeof(int),  1, 512, VisParamDescr.SpecialMode.None, 16),
					//new VisParamDescr("Font Color",		P_COLOR,	typeof(string), "", "FFFFFF", VisParamDescr.SpecialMode.None, ""),
					new VisParamDescr("Font Color R",	P_COLOR_R,	typeof(int),  0, 255, VisParamDescr.SpecialMode.None, 255),
					new VisParamDescr("Font Color G",	P_COLOR_G,	typeof(int),  0, 255, VisParamDescr.SpecialMode.None, 255),
					new VisParamDescr("Font Color B",	P_COLOR_B,	typeof(int),  0, 255, VisParamDescr.SpecialMode.None, 255),
				}
			)
		};

		// IPlugin
		public void Prepare(IApplication appRef, byte[] fileData, AddressTranslate addrTrans){
			this.mAppRef	= appRef;
			this.mFileData	= fileData;
		}

		// IPlugin
		public void Unprepare(){
			this.mAppRef	= null;
			this.mFileData	= null;
		}

		// IPlugin_Visualizer
		public VisDescr[] GetVisGenDescrs(){
			if(this.mFileData == null){
				return null;
			}
			return this.mDescriptors;
		}

		// IPlugin_Visualizer
		public IVisualization2d Generate2d(VisDescr descr, ReadOnlyDictionary<string, object> parms){
			switch(descr.Ident){
				case VIS_GEN_BITMAP:
					return this.GenerateBitmap(parms);
				default:
					this.mAppRef.ReportError("Unknown ident " + descr.Ident);
					return null;
			}
		}

		private IVisualization2d GenerateBitmap(ReadOnlyDictionary<string, object> parms){
			int	offset		= Util.GetFromObjDict(parms, P_OFFSET,		0);
			int	fontWidth	= Util.GetFromObjDict(parms, P_FONTWIDTH,	1);
			int	fontHeight	= Util.GetFromObjDict(parms, P_FONTHEIGHT,	1);
			int	tileWidth	= Util.GetFromObjDict(parms, P_TILEWIDTH,	0x10);
			int	tileHeight	= Util.GetFromObjDict(parms, P_TILEHEIGHT,	0x0F);
			int	fontColorR	= Util.GetFromObjDict(parms, P_COLOR_R,		0xFF);
			int	fontColorG	= Util.GetFromObjDict(parms, P_COLOR_G,		0xFF);
			int	fontColorB	= Util.GetFromObjDict(parms, P_COLOR_B,		0xFF);
			//string	colorString	= Util.GetFromObjDict<string>(parms, P_COLOR,		"");
			//string	colorString	= "FFFFFF";
			//UInt32	colorInteger	= 0x00000000;

			int	charPerBytes	= ((fontWidth * fontHeight) + 7) / 8;
			int	dataLength	= charPerBytes * tileWidth * tileHeight;

			// Check parameters.
			//int	lastAddress	= offset + dataLength - 1;
			//if((offset < 0) || (lastAddress >= this.mFileData.Length)){
			//	this.mAppRef.ReportError("Invalid parameter");
			//	return null;
			//}

			//if(!UInt32.TryParse(colorString, System.Globalization.NumberStyles.HexNumber, null, out colorInteger)){
			//	this.mAppRef.ReportError("Invalid parameter");
			//	return null;
			//}
			//colorInteger	= 0xFF000000U | colorInteger;


			// Generate bitmap
			int		bitmapWidth	= fontWidth * tileWidth;
			int		bitmapHeight	= fontHeight * tileHeight;
			PaletteBitmap	bitmap		= new PaletteBitmap((uint)(bitmapWidth), (uint)(bitmapHeight));

			// Set palette.
			int		colorInteger	= Util.MakeARGB(0xFF, fontColorR, fontColorG, fontColorB);
			bitmap.AddColor(0x00000000);			// Transparent
			bitmap.AddColor((int)colorInteger);

			// Convert to pixels.
			for(int row=0; row<tileHeight; row++){
				for(int col=0; col<tileWidth; col++){
					int	x		= col * fontWidth;
					int	y		= row * fontHeight;
					int	drawOffset	= offset + (row * tileWidth + col) * charPerBytes;
					this.GenerateTile(bitmap, drawOffset, fontWidth, fontHeight, x, y);
				}
			}

			return bitmap.Bitmap;
		}

		private void GenerateTile(PaletteBitmap bitmap, int offset, int fontWidth, int fontHeight, int drawX, int drawY){
			for(int fy = 0; fy < fontHeight; fy++){
				uint	dy	= (uint)(drawY + fy);
				for(int fx = 0; fx < fontWidth; fx++){
					uint	dx		= (uint)(drawX + fx);
					int	pixelIndex	= (fy * fontWidth) + fx;
					int	fontAddress	= offset + (pixelIndex) / 8;
					byte	fontByte	= 0x00;
					if(fontAddress < this.mFileData.Length){
						fontByte	= this.mFileData[fontAddress];
					}

					int	shift		= 7 - (pixelIndex % 8);
					int	fontBit		= (fontByte >> shift) & 0x01;
					bitmap.SetPixel(dx, dy, (byte)fontBit);
				}
			}
		}
	}

	// Class to manage palette bitmap.
	// Because duplicate colors cannot be registered.
	internal class PaletteBitmap{
		public VisBitmap8	Bitmap		{ get; private set; }
		public byte		PaletteCount	{ get; private set; }
		public byte		ColorCount	{ get; private set; }
		private int[]		paletteColor;
		private byte[]		paletteIndex;

		public PaletteBitmap(uint width, uint height){
			this.Bitmap		= new VisBitmap8((int)width, (int)height);
			this.PaletteCount	= 0;
			this.ColorCount		= 0;
			this.paletteColor	= new int[256];
			this.paletteIndex	= new byte[256];
		}
		public void AddColor(int color){
			this.paletteColor[this.PaletteCount]	= color;
			this.paletteIndex[this.ColorCount]	= this.PaletteCount;

			// Check for duplicate colors.
			bool	newColor	= true;
			for(int i=0; i<this.PaletteCount; i++){
				if(this.paletteColor[i] == color){
					// duplicate
					newColor	= false;
					this.paletteIndex[this.ColorCount]	= (byte)i;
					break;
				}
			}

			if(newColor){
				// add
				this.PaletteCount++;
				this.Bitmap.AddColor(color);
			}

			this.ColorCount++;
		}
		public void SetPixel(uint x, uint y, byte color){
			this.Bitmap.SetPixelIndex((int)x, (int)y, this.paletteIndex[color]);
		}
	}
}
