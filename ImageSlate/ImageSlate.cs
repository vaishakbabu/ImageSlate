using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Web;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;



namespace ImageSlate
{
	class Slate
	{
		
		///<summary>
		///This is a command line application that adds a slate and text to Images (HUD)
		///It is intended to be used in conjunction with playblast/render tool
		///Two arguments are taken - source path and a json file with some data
		///</summary>
		///<param name="args"></param>
		static void Main(string[] args)
		{
			
			//initiate a timer to check the elapsed time
			Stopwatch timer = new Stopwatch();
			timer.Start();
			
			//Path to the Image folder
			string imageLocation = args[0];
			
			//Get all the image files from the location - type JPG
			string[] imgfiles = Directory.GetFiles(imageLocation, "*.jpg");
			
			//Parse the json file
			JObject jObject = JObject.Parse(File.ReadAllText(args[1])); 
			string fov = jObject.GetValue("fov").ToString();  //grab the FOV from the json file
			
			#region: Enter the data to be added to the Image Slate here
			//Collect data to be added
			
			string user_name = Environment.UserName;
			string currTime = DateTime.Now.ToString("dd/MM/yyyy @ HH:mm");
			//get the frame range from first and last image filenames
			string frameRange = imgfiles.First().Split('\\').Last().Split('.')[1] + "-" + imgfiles.Last().Split('\\').Last().Split('.')[1]; 
			//get filename, dept, take from the filename
			string filename = imgfiles.First().Split('\\').Last().Split('.').First();
			string dept = filename.Split('_')[4];
			string take = filename.Split('_')[5];
					
			#endregion
			
			
			#region: Adding slate to all the images
			foreach(string imgfile in imgfiles)
			
			{
				//printour the image being processed
				Console.WriteLine(imgfile);
				//create a new filestream
				FileStream fs = new FileStream(imgfile, FileMode.Open);
				
				//multipliers defined here
				double[] actual_values = {20,290,430,630,800,970,1160,1210};
				double[] multiplier = new double[8];
				int i = 0;
				foreach(double av in actual_values)
				{
					multiplier[i] = av/1280;
					i++;
				}
				
				//grabbing the frame number from filename
				string currFrame = imgfile.Split('\\').Last().Split('.')[1];
				
				//adding the texts to the array
				string[] imgText = { filename, "Take_" + take, "FOV:" + fov, frameRange, currFrame, currTime, dept, user_name};

				//load imagefile using the file stream
				System.Drawing.Image pic = Bitmap.FromStream(fs);
				
				//getting the image details
				int img_width = pic.Width;
				int img_height = pic.Height;
				int slate_height = 28;  //set the slate height
				int textht = img_height - 14;  //set the text y value here
				fs.Close();
				
				
				//get the imagefile in a Graphics object
				Graphics graphicsImage = Graphics.FromImage(pic);
				
				//set the txt alignment based on the coordinates
				StringFormat stringformat = new StringFormat();
				stringformat.Alignment = StringAlignment.Near;
				stringformat.LineAlignment = StringAlignment.Center;
				
				//set the font color/format/size etc
				Color FontColor = ColorTranslator.FromHtml("#ffffff"); //white color
				
				Font arialBold = new Font("arial", 12, FontStyle.Bold);
				
				//Writing the text on the image - each string from the imgText array is written onto the image
				i = 0;
				foreach(string txt in imgText)
				{
					double xval = multiplier[i] * img_width;
					graphicsImage.DrawString(imgText[i], arialBold, new SolidBrush(FontColor), new Point((int)xval, textht), stringformat);
					i++;
				}
				
				//define fill color and line color for the rectangle
				Color FillColor = Color.FromArgb(75, 0, 0, 0); //black with alpha 75
				Color LineColor = Color.FromArgb(0, 0, 0);
				
				//create the rectangle object
				Rectangle slate = new Rectangle(new Point(0, img_height - slate_height), new Size(img_width, slate_height));
				
				//draw and fill the rectangle
				graphicsImage.DrawRectangle(new Pen(new SolidBrush(LineColor)), slate);
				graphicsImage.FillRectangle(new SolidBrush(FillColor), slate);
				
				//delete the existing file
				if (File.Exists(imgfile))
					File.Delete(imgfile);
				
				//save the new file in JPG format
				pic.Save(imgfile, ImageFormat.Jpeg);
				//dispose off the Image object
				pic.Dispose();
				
				
			}	
				#endregion

				//printout the execution time
				Console.WriteLine("Time taken : {0}", timer.Elapsed);
				
		}
	}
}
