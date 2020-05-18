using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace XxmsApp.Piece
{
	public class RoundedButton : Frame
    {


        public Button innerButton = null;
        public EventHandler Clicked = null;

		public RoundedButton (string text = "", EventHandler onClicked = null, bool hasShadow = false, int borderRadius = 25)
		{

            

            HasShadow = hasShadow;
            CornerRadius = borderRadius;
            IsClippedToBounds = true;
            Padding = new Thickness(0);
            Content = innerButton = new Button
            {
                Text = text,
            };

            Clicked = (s, e) =>
            {
                onClicked?.Invoke(this, e);
            };

            innerButton.Clicked += Clicked;


        }               

        public string Text
        {
            get => innerButton.Text;
            set => innerButton.Text = value;
        }

    }

    
}