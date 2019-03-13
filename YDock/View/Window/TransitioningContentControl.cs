using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace YDock.View
{
    [TemplateVisualState(GroupName = PresentationGroup, Name = NormalState)]
    [TemplateVisualState(GroupName = PresentationGroup, Name = DefaultTransitionState)]
    [TemplateVisualState(GroupName = PresentationGroup, Name = ScaleTransitionState)]
    [TemplatePart(Name = PreviousContentPresentationSitePartName, Type = typeof(ContentControl))]
    [TemplatePart(Name = CurrentContentPresentationSitePartName, Type = typeof(ContentControl))]
    public class TransitioningContentControl : ContentControl
    {
        #region Property definitions

        public static readonly DependencyProperty IsRandomProperty =
            DependencyProperty.Register("IsRandom",
                                        typeof(bool),
                                        typeof(TransitioningContentControl),
                                        new PropertyMetadata(false));

        /// <summary>
        ///     Identifies the IsTransitioning dependency property.
        /// </summary>
        public static readonly DependencyProperty IsTransitioningProperty =
            DependencyProperty.Register(
                "IsTransitioning",
                typeof(bool),
                typeof(TransitioningContentControl),
                new PropertyMetadata(OnIsTransitioningPropertyChanged));

        /// <summary>
        ///     Identifies the Transition dependency property.
        /// </summary>
        public static readonly DependencyProperty TransitionProperty =
            DependencyProperty.Register(
                "Transition",
                typeof(string),
                typeof(TransitioningContentControl),
                new PropertyMetadata(ScaleTransitionState, OnTransitionPropertyChanged));

        /// <summary>
        ///     Identifies the RestartTransitionOnContentChange dependency property.
        /// </summary>
        public static readonly DependencyProperty RestartTransitionOnContentChangeProperty =
            DependencyProperty.Register(
                "RestartTransitionOnContentChange",
                typeof(bool),
                typeof(TransitioningContentControl),
                new PropertyMetadata(false, OnRestartTransitionOnContentChangePropertyChanged));

        #endregion

        #region Constants

        public const string DefaultTransitionState = "DefaultTransition";

        public const string DownTransitionState = "DownTransition";

        /// <summary>
        ///     代表缩放状态转换
        /// </summary>
        public const string ScaleTransitionState = "ScaleTransition";

        public const string UpTransitionState = "UpTransition";

        internal const string CurrentContentPresentationSitePartName = "CurrentContentPresentationSite";

        internal const string PreviousContentPresentationSitePartName = "PreviousContentPresentationSite";

        private const string NormalState = "Normal";
        private const string PresentationGroup = "PresentationStates";

        #endregion

        #region Static members

        /// <summary>
        ///     IsTransitioningProperty property changed handler.
        /// </summary>
        /// <param name="d">TransitioningContentControl that changed its IsTransitioning.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnIsTransitioningPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (TransitioningContentControl)d;

            if (!source._allowIsTransitioningWrite)
            {
                source.IsTransitioning = (bool)e.OldValue;
                throw new InvalidOperationException("TransitiotioningContentControl_IsTransitioningReadOnly");
            }
        }

        /// <summary>
        ///     RestartTransitionOnContentChangeProperty property changed handler.
        /// </summary>
        /// <param name="d">TransitioningContentControl that changed its RestartTransitionOnContentChange.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnRestartTransitionOnContentChangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TransitioningContentControl)d).OnRestartTransitionOnContentChangeChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        /// <summary>
        ///     TransitionProperty property changed handler.
        /// </summary>
        /// <param name="d">TransitioningContentControl that changed its Transition.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnTransitionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (TransitioningContentControl)d;
            var oldTransition = e.OldValue as string;
            var newTransition = e.NewValue as string;

            if (source.IsTransitioning)
            {
                source.AbortTransition();
            }

            // find new transition
            var newStoryboard = source.GetStoryboard(newTransition);

            // unable to find the transition.
            if (newStoryboard == null)
            {
                // could be during initialization of xaml that presentationgroups was not yet defined
                if (VisualStates.TryGetVisualStateGroup(source, PresentationGroup) == null)
                {
                    // will delay check
                    source.CurrentTransition = null;
                }
                else
                {
                    // revert to old value
                    source.SetValue(TransitionProperty, oldTransition);

                    throw new ArgumentException(
                        "TransitioningContentControl_TransitionNotFound");
                }
            }
            else
            {
                source.CurrentTransition = newStoryboard;
            }
        }

        #endregion

        /// <summary>
        ///     Indicates whether the control allows writing IsTransitioning.
        /// </summary>
        private bool _allowIsTransitioningWrite;

        /// <summary>
        ///     The storyboard that is used to transition old and new content.
        /// </summary>
        private Storyboard _currentTransition;

        private readonly Random random = new Random();

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransitioningContentControl" /> class.
        /// </summary>
        public TransitioningContentControl()
        {
            DefaultStyleKey = typeof(TransitioningContentControl);
        }

        #endregion

        #region Properties

        public bool IsRandom
        {
            get { return (bool)GetValue(IsRandomProperty); }
            set { SetValue(IsRandomProperty, value); }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is currently performing
        ///     a transition.
        /// </summary>
        public bool IsTransitioning
        {
            get { return (bool)GetValue(IsTransitioningProperty); }
            private set
            {
                _allowIsTransitioningWrite = true;
                SetValue(IsTransitioningProperty, value);
                _allowIsTransitioningWrite = false;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the current transition
        ///     will be aborted when setting new content during a transition.
        /// </summary>
        public bool RestartTransitionOnContentChange
        {
            get { return (bool)GetValue(RestartTransitionOnContentChangeProperty); }
            set { SetValue(RestartTransitionOnContentChangeProperty, value); }
        }

        /// <summary>
        ///     Gets or sets the name of the transition to use. These correspond
        ///     directly to the VisualStates inside the PresentationStates group.
        /// </summary>
        public string Transition
        {
            get { return GetValue(TransitionProperty) as string; }
            set { SetValue(TransitionProperty, value); }
        }

        /// <summary>
        ///     Gets or sets the current content presentation site.
        /// </summary>
        /// <value>The current content presentation site.</value>
        private ContentPresenter CurrentContentPresentationSite { get; set; }

        /// <summary>
        ///     Gets or sets the storyboard that is used to transition old and new content.
        /// </summary>
        private Storyboard CurrentTransition
        {
            get { return _currentTransition; }
            set
            {
                // decouple event
                if (_currentTransition != null)
                {
                    _currentTransition.Completed -= OnTransitionCompleted;
                }

                _currentTransition = value;

                if (_currentTransition != null)
                {
                    _currentTransition.Completed += OnTransitionCompleted;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the previous content presentation site.
        /// </summary>
        /// <value>The previous content presentation site.</value>
        private ContentPresenter PreviousContentPresentationSite { get; set; }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when the current transition has completed.
        /// </summary>
        public event RoutedEventHandler TransitionCompleted;

        #endregion

        #region Override members

        /// <summary>
        ///     Builds the visual tree for the TransitioningContentControl control
        ///     when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (IsTransitioning)
            {
                AbortTransition();
            }

            base.OnApplyTemplate();

            PreviousContentPresentationSite = GetTemplateChild(PreviousContentPresentationSitePartName) as ContentPresenter;
            CurrentContentPresentationSite = GetTemplateChild(CurrentContentPresentationSitePartName) as ContentPresenter;

            if (CurrentContentPresentationSite != null)
            {
                CurrentContentPresentationSite.Content = Content;
            }

            var _transitionState = default(string);
            if (!IsRandom)
            {
                _transitionState = Transition;
            }
            else
            {
                var value = random.Next() % 4;
                switch (value)
                {
                    case 0:
                        _transitionState = DefaultTransitionState;
                        break;
                    case 1:
                        _transitionState = UpTransitionState;
                        break;
                    case 2:
                        _transitionState = DownTransitionState;
                        break;
                    case 3:
                        _transitionState = ScaleTransitionState;
                        break;
                }
            }

            // hookup currenttransition
            var transition = GetStoryboard(_transitionState);

            CurrentTransition = transition;
            if (transition == null)
            {
                var invalidTransition = _transitionState;
                // revert to default
                Transition = DefaultTransitionState;

                throw new ArgumentException(
                    "TransitioningContentControl_TransitionNotFound");
            }

            VisualStateManager.GoToState(this, NormalState, false);
            VisualStateManager.GoToState(this, _transitionState, true);
        }

        /// <summary>
        ///     Called when the value of the <see cref="P:System.Windows.Controls.ContentControl.Content" /> property changes.
        /// </summary>
        /// <param name="oldContent">The old value of the <see cref="P:System.Windows.Controls.ContentControl.Content" /> property.</param>
        /// <param name="newContent">The new value of the <see cref="P:System.Windows.Controls.ContentControl.Content" /> property.</param>
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            //注意这里设置Content为null，断开逻辑父级保证YDock可以正常进行
            if (oldContent != null && newContent == null)
            {
                CurrentContentPresentationSite.Content = null;
                PreviousContentPresentationSite.Content = null;
                return;
            }

            StartTransition(oldContent, newContent);
        }

        #endregion

        #region Event handlers

        /// <summary>
        ///     Handles the Completed event of the transition storyboard.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void OnTransitionCompleted(object sender, EventArgs e)
        {
            AbortTransition();

            var handler = TransitionCompleted;
            if (handler != null)
            {
                handler(this, new RoutedEventArgs());
            }
        }

        #endregion

        #region Members

        /// <summary>
        ///     Aborts the transition and releases the previous content.
        /// </summary>
        public void AbortTransition()
        {
            // go to normal state and release our hold on the old content.
            VisualStateManager.GoToState(this, NormalState, false);
            IsTransitioning = false;
            if (PreviousContentPresentationSite != null)
            {
                PreviousContentPresentationSite.Content = null;
            }
        }

        /// <summary>
        ///     Called when the RestartTransitionOnContentChangeProperty changes.
        /// </summary>
        /// <param name="oldValue">The old value of RestartTransitionOnContentChange.</param>
        /// <param name="newValue">The new value of RestartTransitionOnContentChange.</param>
        protected virtual void OnRestartTransitionOnContentChangeChanged(bool oldValue, bool newValue)
        {
        }

        /// <summary>
        ///     Attempts to find a storyboard that matches the newTransition name.
        /// </summary>
        /// <param name="newTransition">The new transition.</param>
        /// <returns>A storyboard or null, if no storyboard was found.</returns>
        private Storyboard GetStoryboard(string newTransition)
        {
            var presentationGroup = VisualStates.TryGetVisualStateGroup(this, PresentationGroup);
            Storyboard newStoryboard = null;
            if (presentationGroup != null)
            {
                newStoryboard = presentationGroup.States
                                                 .OfType<VisualState>()
                                                 .Where(state => state.Name == newTransition)
                                                 .Select(state => state.Storyboard)
                                                 .FirstOrDefault();
            }

            return newStoryboard;
        }

        /// <summary>
        ///     Starts the transition.
        /// </summary>
        /// <param name="oldContent">The old content.</param>
        /// <param name="newContent">The new content.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "newContent", Justification = "Should be used in the future.")]
        private void StartTransition(object oldContent, object newContent)
        {
            // both presenters must be available, otherwise a transition is useless.
            if (CurrentContentPresentationSite != null && PreviousContentPresentationSite != null)
            {
                CurrentContentPresentationSite.Content = newContent;

                PreviousContentPresentationSite.Content = oldContent;

                // and start a new transition
                if (!IsTransitioning || RestartTransitionOnContentChange)
                {
                    IsTransitioning = true;
                    VisualStateManager.GoToState(this, NormalState, false);
                    VisualStateManager.GoToState(this, Transition, true);
                }
            }
        }

        #endregion
    }
}