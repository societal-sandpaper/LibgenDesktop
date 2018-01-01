﻿using System;
using System.Collections.ObjectModel;
using System.Threading;
using LibgenDesktop.Infrastructure;
using LibgenDesktop.Models;
using LibgenDesktop.Models.Entities;
using LibgenDesktop.Models.ProgressArgs;
using LibgenDesktop.Models.Utils;
using static LibgenDesktop.Models.Settings.AppSettings;

namespace LibgenDesktop.ViewModels
{
    internal class NonFictionSearchResultsTabViewModel : TabViewModel
    {
        private readonly NonFictionColumnSettings columnSettings;
        private ObservableCollection<NonFictionBook> books;
        private string searchQuery;
        private string bookCount;
        private bool isBookGridVisible;
        private bool isSearchProgressPanelVisible;
        private string searchProgressStatus;
        private bool isStatusBarVisible;

        public NonFictionSearchResultsTabViewModel(MainModel mainModel, IWindowContext mainWindowContext, string searchQuery, ObservableCollection<NonFictionBook> searchResults)
            : base(mainModel, mainWindowContext, searchQuery)
        {
            columnSettings = mainModel.AppSettings.NonFiction.Columns;
            this.searchQuery = searchQuery;
            books = searchResults;
            OpenDetailsCommand = new Command(param => OpenDetails(param as NonFictionBook));
            SearchCommand = new Command(Search);
            BookDataGridEnterKeyCommand = new Command(BookDataGridEnterKeyPressed);
            Initialize();
        }

        public string SearchQuery
        {
            get
            {
                return searchQuery;
            }
            set
            {
                searchQuery = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<NonFictionBook> Books
        {
            get
            {
                return books;
            }
            set
            {
                books = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsBookGridVisible
        {
            get
            {
                return isBookGridVisible;
            }
            set
            {
                isBookGridVisible = value;
                NotifyPropertyChanged();
            }
        }

        public int TitleColumnWidth
        {
            get
            {
                return columnSettings.TitleColumnWidth;
            }
            set
            {
                columnSettings.TitleColumnWidth = value;
            }
        }

        public int AuthorsColumnWidth
        {
            get
            {
                return columnSettings.AuthorsColumnWidth;
            }
            set
            {
                columnSettings.AuthorsColumnWidth = value;
            }
        }

        public int SeriesColumnWidth
        {
            get
            {
                return columnSettings.SeriesColumnWidth;
            }
            set
            {
                columnSettings.SeriesColumnWidth = value;
            }
        }

        public int YearColumnWidth
        {
            get
            {
                return columnSettings.YearColumnWidth;
            }
            set
            {
                columnSettings.YearColumnWidth = value;
            }
        }

        public int PublisherColumnWidth
        {
            get
            {
                return columnSettings.PublisherColumnWidth;
            }
            set
            {
                columnSettings.PublisherColumnWidth = value;
            }
        }

        public int FormatColumnWidth
        {
            get
            {
                return columnSettings.FormatColumnWidth;
            }
            set
            {
                columnSettings.FormatColumnWidth = value;
            }
        }

        public int FileSizeColumnWidth
        {
            get
            {
                return columnSettings.FileSizeColumnWidth;
            }
            set
            {
                columnSettings.FileSizeColumnWidth = value;
            }
        }

        public int OcrColumnWidth
        {
            get
            {
                return columnSettings.OcrColumnWidth;
            }
            set
            {
                columnSettings.OcrColumnWidth = value;
            }
        }

        public bool IsSearchProgressPanelVisible
        {
            get
            {
                return isSearchProgressPanelVisible;
            }
            set
            {
                isSearchProgressPanelVisible = value;
                NotifyPropertyChanged();
            }
        }

        public string SearchProgressStatus
        {
            get
            {
                return searchProgressStatus;
            }
            set
            {
                searchProgressStatus = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsStatusBarVisible
        {
            get
            {
                return isStatusBarVisible;
            }
            set
            {
                isStatusBarVisible = value;
                NotifyPropertyChanged();
            }
        }

        public string BookCount
        {
            get
            {
                return bookCount;
            }
            set
            {
                bookCount = value;
                NotifyPropertyChanged();
            }
        }

        public NonFictionBook SelectedBook { get; set; }

        public Command OpenDetailsCommand { get; }
        public Command SearchCommand { get; }
        public Command BookDataGridEnterKeyCommand { get; }

        private void Initialize()
        {
            isBookGridVisible = true;
            isStatusBarVisible = true;
            isSearchProgressPanelVisible = false;
            UpdateBookCount();
            Events.RaiseEvent(ViewModelEvent.RegisteredEventId.FOCUS_SEARCH_TEXT_BOX);
        }

        private void UpdateBookCount()
        {
            BookCount = $"Найдено книг: {Books.Count.ToFormattedString()}";
        }

        private void BookDataGridEnterKeyPressed()
        {
            OpenDetails(SelectedBook);
        }

        private void OpenDetails(NonFictionBook book)
        {
            NonFictionDetailsWindowViewModel detailsWindowViewModel = new NonFictionDetailsWindowViewModel(MainModel, book);
            IWindowContext detailsWindowContext = WindowManager.CreateWindow(RegisteredWindows.WindowKey.NON_FICTION_DETAILS_WINDOW, detailsWindowViewModel, MainWindowContext);
            NonFictionDetailsWindowSettings detailsWindowSettings = MainModel.AppSettings.NonFiction.DetailsWindow;
            detailsWindowContext.ShowDialog(detailsWindowSettings.Width, detailsWindowSettings.Height);
        }

        private async void Search()
        {
            if (!String.IsNullOrWhiteSpace(SearchQuery) && !IsSearchProgressPanelVisible)
            {
                Title = SearchQuery;
                IsBookGridVisible = false;
                IsStatusBarVisible = false;
                IsSearchProgressPanelVisible = true;
                UpdateSearchProgressStatus(0);
                Progress<SearchProgress> searchProgressHandler = new Progress<SearchProgress>(HandleSearchProgress);
                CancellationToken cancellationToken = new CancellationToken();
                ObservableCollection<NonFictionBook> result = new ObservableCollection<NonFictionBook>();
                try
                {
                    result = await MainModel.SearchNonFictionAsync(SearchQuery, searchProgressHandler, cancellationToken);
                }
                catch (Exception exception)
                {
                    ShowErrorWindow(exception);
                }
                Books = result;
                UpdateBookCount();
                IsSearchProgressPanelVisible = false;
                IsBookGridVisible = true;
                IsStatusBarVisible = true;
            }
        }

        private void HandleSearchProgress(SearchProgress searchProgress)
        {
            UpdateSearchProgressStatus(searchProgress.ItemsFound);
        }

        private void UpdateSearchProgressStatus(int booksFound)
        {
            SearchProgressStatus = $"Найдено книг: {booksFound.ToFormattedString()}";
        }
    }
}