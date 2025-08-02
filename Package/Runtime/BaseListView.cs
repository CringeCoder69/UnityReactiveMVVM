using System.Collections.Generic;
using System.Collections.Specialized;
using ObservableCollections;
using UnityEngine;
using UnityEngine.Pool;

namespace UnityReactiveMVVM
{
    public class BaseListView<TView, TViewModel> : BaseView<IObservableCollection<TViewModel>>
        where TView : BaseView<TViewModel>
        where TViewModel : class
    {
        [SerializeField] private TView _prototype;

        private readonly Dictionary<TViewModel, TView> _map = new();
        private readonly Dictionary<TView, TViewModel> _inverseMap = new();
        private readonly List<TView> _listMap = new();

        private ObjectPool<TView> _pool;

        private void Awake()
        {
            _pool = new ObjectPool<TView>(() => Instantiate(_prototype, transform),
                x => x.gameObject.SetActive(true),
                x =>
                {
                    x.gameObject.SetActive(false);
                    x.transform.SetAsLastSibling();
                },
                x => Destroy(x.gameObject));
        }

        protected sealed override void InitializeNested(IObservableCollection<TViewModel> viewModel)
        {
            base.InitializeNested(viewModel);
        }

        protected sealed override void BindSelf(IObservableCollection<TViewModel> viewModel)
        {
            base.BindSelf(viewModel);

            Clear();
            int index = 0;
            foreach (var item in viewModel)
                Add(item, index++);

            Bind(viewModel.ObserveChanged(), OnCollectionChanged);
        }

        private void OnCollectionChanged(CollectionChangedEvent<TViewModel> ev)
        {
            switch (ev.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        Add(ev.NewItem, ev.NewStartingIndex);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    {
                        var view = _map[ev.OldItem];
                        _pool.Release(view);

                        _map.Remove(ev.OldItem);
                        _inverseMap.Remove(view);
                        _listMap.RemoveAt(ev.OldStartingIndex);
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    {
                        var view = _listMap[ev.OldStartingIndex];
                        view.transform.SetSiblingIndex(ev.NewStartingIndex);

                        _listMap.RemoveAt(ev.OldStartingIndex);
                        _listMap.Insert(ev.NewStartingIndex, view);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    {
                        var view = _map[ev.OldItem];
                        _map.Remove(ev.OldItem);

                        view.Initialize(ev.NewItem);
                        _map.Add(ev.NewItem, view);

                        _inverseMap[view] = ev.NewItem;
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    {
                        if (ev.SortOperation.IsClear)
                        {
                            Clear();
                        }
                        else if (ev.SortOperation.IsReverse)
                        {
                            _listMap.Reverse(ev.SortOperation.Index, ev.SortOperation.Count);
                            Sort(ev.SortOperation.Index, ev.SortOperation.Count);
                        }
                        else if (ev.SortOperation.IsSort)
                        {
                            _listMap.Sort(ev.SortOperation.Index, ev.SortOperation.Count,
                                new Comparer(_inverseMap, ev.SortOperation.Comparer));
                            Sort(ev.SortOperation.Index, ev.SortOperation.Count);
                        }
                    }
                    break;
            }
        }

        private void Add(TViewModel viewModel, int index)
        {
            var view = _pool.Get();
            view.transform.SetSiblingIndex(index);
            view.Initialize(viewModel);

            _map.Add(viewModel, view);
            _inverseMap.Add(view, viewModel);
            _listMap.Insert(index, view);
        }

        private void Clear()
        {
            foreach (var view in _map.Values)
                _pool.Release(view);
            _map.Clear();
        }

        private void Sort(int index, int count)
        {
            for (int i = index; i < index + count; i++)
                _listMap[i].transform.SetSiblingIndex(i);
        }

        private class Comparer : IComparer<TView>
        {
            private readonly IDictionary<TView, TViewModel> _map;
            private readonly IComparer<TViewModel> _comparer;

            public Comparer(IDictionary<TView, TViewModel> map, IComparer<TViewModel> comparer)
            {
                _map = map;
                _comparer = comparer;
            }

            public int Compare(TView x, TView y)
            {
                return _comparer.Compare(_map[x], _map[y]);
            }
        }
    }
}
