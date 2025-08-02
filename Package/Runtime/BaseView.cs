using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace UnityReactiveMVVM
{
    public abstract class BaseView<TViewModel> : MonoBehaviour where TViewModel : class
    {
        private DisposableBag _disposableBag;
        private TViewModel _viewModel;

        public void Initialize(TViewModel viewModel)
        {
            if (EqualityComparer<TViewModel>.Default.Equals(viewModel, _viewModel))
                return;

            _viewModel = viewModel;
            _disposableBag.Clear();

            if (viewModel == null)
                return;

            InitializeNested(viewModel);
            if (isActiveAndEnabled)
                BindSelf(viewModel);
        }

        private void OnEnable()
        {
            if (_viewModel != null)
                BindSelf(_viewModel);
        }

        private void OnDisable() => _disposableBag.Clear();

        protected virtual void InitializeNested(TViewModel viewModel) { }
        protected virtual void BindSelf(TViewModel viewModel) { }

        protected void Bind<TProperty>(Observable<TProperty> property, Action<TProperty> onNext) =>
            property.Subscribe(onNext).AddTo(ref _disposableBag);
    }
}
