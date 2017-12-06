﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Xamarin.Forms.Platform.WinForms
{
	public class VisualElementRenderer<TElement, TNativeElement> :
		IVisualElementRenderer, IDisposable, IEffectControlProvider
		where TElement : VisualElement
		where TNativeElement : Control
	{
		VisualElementTracker<TElement, TNativeElement> _tracker;
		readonly VisualElementRendererCollection _children = new VisualElementRendererCollection();

		public void Dispose()
		{
			foreach (var item in _children)
			{
				item?.Dispose();
			}
			_children.Clear();
		}

		protected VisualElementTracker<TElement, TNativeElement> Tracker
		{
			get { return _tracker; }
			set
			{
				if (_tracker == value)
					return;

				if (_tracker != null)
				{
					_tracker.Dispose();
					_tracker.Updated -= OnTrackerUpdated;
				}

				_tracker = value;

				if (_tracker != null)
				{
					_tracker.Updated += OnTrackerUpdated;
					UpdateTracker();
				}
			}
		}

		VisualElementPackager Packager { get; set; }

		public TNativeElement Control { get; private set; }

		public TElement Element { get; private set; }

		protected bool AutoPackage { get; set; } = true;

		protected bool AutoTrack { get; set; } = true;


		protected virtual void OnElementChanged(ElementChangedEventArgs<TElement> e)
		{
			var args = new VisualElementChangedEventArgs(e.OldElement, e.NewElement);
			ElementChanged?.Invoke(this, args);
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateEnabled();
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();
			/*
			else if (e.PropertyName == AutomationProperties.HelpTextProperty.PropertyName)
				SetAutomationPropertiesHelpText();
			else if (e.PropertyName == AutomationProperties.NameProperty.PropertyName)
				SetAutomationPropertiesName();
			else if (e.PropertyName == AutomationProperties.IsInAccessibleTreeProperty.PropertyName)
				SetAutomationPropertiesAccessibilityView();
			else if (e.PropertyName == AutomationProperties.LabeledByProperty.PropertyName)
				SetAutomationPropertiesLabeledBy();
			*/
		}

		protected void SetNativeControl(TNativeElement control)
		{
			TNativeElement oldControl = Control;
			Control = control;
			_children.ParentNativeElement = control;

			if (oldControl != null)
			{
				//oldControl.Loaded -= OnControlLoaded;
				oldControl.GotFocus -= OnControlGotFocus;
				oldControl.LostFocus -= OnControlLostFocus;
			}

			UpdateTracker();

			if (control == null)
				return;

			//Control.HorizontalAlignment = HorizontalAlignment.Stretch;
			//Control.VerticalAlignment = VerticalAlignment.Stretch;

			if (Element == null)
				throw new InvalidOperationException(
					"Cannot assign a native control without an Element; Renderer unbound and/or disposed. " +
					"Please consult Xamarin.Forms renderers for reference implementation of OnElementChanged.");

			Element.IsNativeStateConsistent = false;
			//control.Loaded += OnControlLoaded;

			//	OnCotrolLoaded が呼ばれないので同等の処理を呼んでおく
			//	これを設定しないと子要素のレイアウト処理が呼ばれない
			Element.IsNativeStateConsistent = true;

			control.GotFocus += OnControlGotFocus;
			control.LostFocus += OnControlLostFocus;

			UpdateBackgroundColor();

			//if (Element != null && !string.IsNullOrEmpty(Element.AutomationId))
			//	SetAutomationId(Element.AutomationId);
		}

		protected virtual void UpdateBackgroundColor()
		{
			Color backgroundColor = Element.BackgroundColor;
			var control = Control as Control;
			if (control != null)
			{
				if (!backgroundColor.IsDefault)
				{
					control.BackColor = backgroundColor.ToWindowsColor();
				}
				else
				{
					control.BackColor = System.Drawing.SystemColors.Window;
				}
			}
		}

		protected virtual void UpdateNativeControl()
		{
			UpdateEnabled();
			/*
			SetAutomationPropertiesHelpText();
			SetAutomationPropertiesName();
			SetAutomationPropertiesAccessibilityView();
			SetAutomationPropertiesLabeledBy();
			*/
		}

		void OnControlGotFocus(object sender, EventArgs args)
		{
			((IVisualElementController)Element).SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
		}

		void OnControlLoaded(object sender, EventArgs args)
		{
			Element.IsNativeStateConsistent = true;
		}

		void OnControlLostFocus(object sender, EventArgs args)
		{
			((IVisualElementController)Element).SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
		}

		internal virtual void OnElementFocusChangeRequested(object sender, VisualElement.FocusRequestArgs args)
		{
			var control = Control as Control;
			if (control == null)
				return;

			/*
			if (args.Focus)
				args.Result = control.Focus(FocusState.Programmatic);
			else
			{
				UnfocusControl(control);
				args.Result = true;
			}
			*/
		}

		void OnTrackerUpdated(object sender, EventArgs e)
		{
			UpdateNativeControl();
		}

		void UpdateEnabled()
		{
			var control = Control as Control;
			if (control != null)
				control.Enabled = Element.IsEnabled;
			/*else
				IsHitTestVisible = Element.IsEnabled && !Element.InputTransparent;*/
		}

		void UpdateTracker()
		{
			if (_tracker == null)
				return;

			//_tracker.PreventGestureBubbling = PreventGestureBubbling;
			_tracker.Control = Control;
			_tracker.Element = Element;
		}


		#region IVisualElementRenderer

		public VisualElementRendererCollection Children => _children;

		VisualElement IVisualElementRenderer.Element => Element;

		Control IVisualElementRenderer.NativeElement => Control;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			//	暫定
			var control = Control;
			if (control != null)
			{
				return new SizeRequest(
					new Size(
						Math.Min(control.Width, widthConstraint),
						Math.Min(control.Height, heightConstraint)));
			}
			return new SizeRequest(new Size(widthConstraint, heightConstraint));
		}


		public void SetElement(VisualElement element)
		{
			TElement oldElement = Element;
			Element = (TElement)element;

			if (oldElement != null)
			{
				oldElement.PropertyChanged -= OnElementPropertyChanged;
				oldElement.FocusChangeRequested -= OnElementFocusChangeRequested;
			}

			if (element != null)
			{
				Element.PropertyChanged += OnElementPropertyChanged;
				Element.FocusChangeRequested += OnElementFocusChangeRequested;

				if (AutoPackage && Packager == null)
					Packager = new VisualElementPackager(this);

				if (AutoTrack && Tracker == null)
				{
					Tracker = new VisualElementTracker<TElement, TNativeElement>();
				}

				// Disabled until reason for crashes with unhandled exceptions is discovered
				// Without this some layouts may end up with improper sizes, however their children
				// will position correctly
				//Loaded += (sender, args) => {
				if (Packager != null)
					Packager.Load();
				//};
			}

			OnElementChanged(new ElementChangedEventArgs<TElement>(oldElement, Element));

			var controller = (IElementController)oldElement;
			if (controller != null && controller.EffectControlProvider == this)
			{
				controller.EffectControlProvider = null;
			}

			controller = element;
			if (controller != null)
				controller.EffectControlProvider = this;
		}

		#endregion

		#region IEffectControlProvider

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			throw new NotImplementedException();
		}

		#endregion

	}
}
