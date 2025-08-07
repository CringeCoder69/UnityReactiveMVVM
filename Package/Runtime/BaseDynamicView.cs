using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace UnityReactiveMVVM
{
    public abstract class BaseDynamicView<TBaseViewModel> : BaseView<Observable<TBaseViewModel>>
    {
        [SerializeField] private GameObject _fallback;

        private readonly Dictionary<Type, IViewHolder> _viewHolders = new();

        private void Awake()
        {
            InitializeSubViewMap();
        }

        protected sealed override void InitializeNested(Observable<TBaseViewModel> viewModel)
        {
            base.InitializeNested(viewModel);
        }

        protected sealed override void BindSelf(Observable<TBaseViewModel> viewModel, ref DisposableBag disposableBag)
        {
            base.BindSelf(viewModel, ref disposableBag);
            viewModel.Subscribe(OnViewModelChanged).AddTo(ref disposableBag);
        }

        private void OnViewModelChanged(TBaseViewModel nextViewModel)
        {
            var hasView = false;
            foreach (var viewHolder in _viewHolders.Values)
                hasView |= viewHolder.Handle(nextViewModel);

            if (_fallback != null)
                _fallback.SetActive(!hasView);
        }

        protected virtual void InitializeSubViewMap() { }

        protected void RegisterView<TViewModel>(BaseView<TViewModel> view)
            where TViewModel : class, TBaseViewModel
        {
            view.gameObject.SetActive(false);
            _viewHolders.Add(typeof(TViewModel), new ViewHolder<TViewModel>(view));
        }

        private interface IViewHolder
        {
            bool Handle(TBaseViewModel viewModel);
        }

        private class ViewHolder<TViewModel> : IViewHolder
            where TViewModel : class, TBaseViewModel
        {
            private readonly BaseView<TViewModel> _view;

            public ViewHolder(BaseView<TViewModel> view)
            {
                _view = view;
            }

            public bool Handle(TBaseViewModel baseViewModel)
            {
                var viewModel = baseViewModel as TViewModel;
                var hasView = viewModel != null;

                _view.Initialize(viewModel);
                _view.gameObject.SetActive(hasView);

                return hasView;
            }
        }
    }
}
