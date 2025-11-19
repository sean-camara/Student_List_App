using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using MAUIApp7.Models;
using Microsoft.Maui.Graphics;
using System.Linq;
using System.Threading.Tasks;

namespace MAUIApp7
{
    public partial class MainPage : ContentPage
    {
        private ObservableCollection<Student> _students = new ObservableCollection<Student>();

        // Pale, consistent card color
        private readonly Color CardBackground = Color.FromArgb("#F8F6F0");

        // Paging
        private const int PageSize = 50;
        private int _loadedCount = 0;
        private bool _isLoading = false;
        private bool _hasMore = true;

        public MainPage()
        {
            InitializeComponent();
            sectionList.ItemsSource = _students;
            // fire-and-forget initial load; exceptions are handled inside LoadNextPageAsync
            _ = LoadNextPageAsync();
        }

        // Add student (async)
        private async void OnNewButtonClicked(object sender, EventArgs e)
        {
            statusMessage.Text = string.Empty;

            var name = newStudent.Text?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                statusMessage.Text = "Please enter a name.";
                return;
            }

            // Add to DB off UI thread
            var added = await App.StudentRepo.AddNewStudentAsync(name);
            statusMessage.Text = App.StudentRepo.StatusMessage;

            if (added != null)
            {
                // prepare UI-only props
                added.CardColor = CardBackground;
                added.Initial = ComputeInitial(added.Name);
                added.Subtitle = $"ID #{added.Id}";

                // insert at top so user sees new item immediately
                _students.Insert(0, added);
                _loadedCount++;
            }

            newStudent.Text = string.Empty;
        }

        // Manual refresh button
        private async void OnGetButtonClicked(object sender, EventArgs e)
        {
            statusMessage.Text = string.Empty;
            await RefreshAllAsync();
        }

        // Load next page (incremental)
        private async Task LoadNextPageAsync()
        {
            if (_isLoading || !_hasMore) return;
            _isLoading = true;

            var list = await App.StudentRepo.GetSectionAsync(_loadedCount, PageSize);
            if (list == null || list.Count == 0)
            {
                _hasMore = false;
                _isLoading = false;
                return;
            }

            foreach (var s in list)
            {
                s.CardColor = CardBackground;
                s.Initial = ComputeInitial(s.Name);
                s.Subtitle = $"ID #{s.Id}";
                _students.Add(s);
            }

            _loadedCount += list.Count;
            if (list.Count < PageSize) _hasMore = false;

            _isLoading = false;
        }

        // Called by RemainingItemsThresholdReached in XAML
        private async void OnRemainingItemsThresholdReached(object sender, EventArgs e)
        {
            await LoadNextPageAsync();
        }

        // Full refresh (clears and loads first page)
        private async Task RefreshAllAsync()
        {
            _students.Clear();
            _loadedCount = 0;
            _hasMore = true;
            await LoadNextPageAsync();
        }

        private string ComputeInitial(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var first = parts.FirstOrDefault();
            if (string.IsNullOrEmpty(first)) return name.Substring(0, 1).ToUpper();
            return first.Substring(0, 1).ToUpper();
        }

        // Edit handler: prompt, update DB async, then replace item in collection to force refresh
        private async void OnEditClicked(object sender, EventArgs e)
        {
            try
            {
                var btn = sender as Button;
                if (btn?.BindingContext is not Student student) return;

                string result = await DisplayPromptAsync("Edit student", "Update name:", initialValue: student.Name, maxLength: 250, keyboard: Keyboard.Text);
                if (string.IsNullOrWhiteSpace(result)) return;

                var newName = result.Trim();

                // update DB
                student.Name = newName; // update object used for DB call
                var ok = await App.StudentRepo.UpdateStudentAsync(student);
                statusMessage.Text = App.StudentRepo.StatusMessage;

                if (!ok) return;

                // Create replacement instance (avoids needing INotifyPropertyChanged)
                var replacement = new Student
                {
                    Id = student.Id,
                    Name = newName,
                    CardColor = CardBackground,
                    Initial = ComputeInitial(newName),
                    Subtitle = $"ID #{student.Id}"
                };

                var existing = _students.FirstOrDefault(x => x.Id == student.Id);
                if (existing != null)
                {
                    var idx = _students.IndexOf(existing);
                    _students[idx] = replacement;
                }
            }
            catch (Exception ex)
            {
                statusMessage.Text = $"Error: {ex.Message}";
            }
        }

        // Delete handler: confirm, delete DB async, then remove from ObservableCollection
        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            try
            {
                var btn = sender as Button;
                if (btn?.BindingContext is not Student student) return;

                bool ok = await DisplayAlert("Delete", $"Delete \"{student.Name}\"?", "Delete", "Cancel");
                if (!ok) return;

                var deleted = await App.StudentRepo.DeleteStudentAsync(student.Id);
                statusMessage.Text = App.StudentRepo.StatusMessage;

                if (deleted)
                {
                    var existing = _students.FirstOrDefault(x => x.Id == student.Id);
                    if (existing != null) _students.Remove(existing);
                    // if you want, decrease _loadedCount so paging stays accurate
                    _loadedCount = Math.Max(0, _loadedCount - 1);
                }
            }
            catch (Exception ex)
            {
                statusMessage.Text = $"Error: {ex.Message}";
            }
        }
    }
}