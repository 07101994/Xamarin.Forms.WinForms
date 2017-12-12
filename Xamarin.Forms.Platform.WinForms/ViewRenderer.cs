﻿using System.Windows.Forms;

namespace Xamarin.Forms.Platform.WinForms
{
	public class ViewRenderer<TElement, TNativeElement> :
		VisualElementRenderer<TElement, TNativeElement>
		where TElement : View
		where TNativeElement : Control, new()
	{
		protected override void OnElementChanged(ElementChangedEventArgs<TElement> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var control = new TNativeElement();
					control.Anchor = AnchorStyles.Left | AnchorStyles.Top;
					control.SetBounds(0, 0, 0, 0, BoundsSpecified.All);
					SetNativeControl(control);
				}
				UpdateBackgroundColor();
			}
		}

	}
}