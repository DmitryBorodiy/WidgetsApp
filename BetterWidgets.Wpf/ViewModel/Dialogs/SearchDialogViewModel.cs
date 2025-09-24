using BetterWidgets.Behaviours.Validators;
using BetterWidgets.Controls;
using BetterWidgets.ViewModel.Components;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace BetterWidgets.ViewModel.Dialogs
{
    public partial class SearchDialogViewModel : ObservableValidator
    {
        public SearchDialogViewModel()
        {
            ErrorsChanged += OnErrorsChanged;
        }

        #region Props

        public Widget Widget { get; set; }

        [ObservableProperty]
        [Required(ErrorMessageResourceType = typeof(Resources.Resources),
                  ErrorMessageResourceName = nameof(Resources.Resources.PlaceNameCannotBeEmpty))]
        [CustomValidation(typeof(SearchDialogViewModel), nameof(ValidateSearch))]
        public string searchText = string.Empty;

        [ObservableProperty]
        public ObservableCollection<SearchView> suggestions;

        [ObservableProperty]
        public SearchView selectedSuggestion;

        [ObservableProperty]
        public DataTemplate suggestionItemTemplate;

        [ObservableProperty]
        public ICommand textChangedCommand;

        #endregion

        #region Events
        public EventHandler<SearchView> SuggestionChosen;
        #endregion

        #region Handlers

        partial void OnSearchTextChanged(string value)
        {
            ValidateProperty(value, nameof(SearchText));
            TextChangedCommand?.Execute(this);
        }

        private void OnErrorsChanged(object sender, DataErrorsChangedEventArgs e)
        {
            if(Widget != null)
               Widget.IsDialogPrimaryButtonEnabled = !HasErrors;
        }

        public static ValidationResult ValidateSearch(string name, ValidationContext context)
        {
            if(context.ObjectInstance is SearchDialogViewModel vm)
            {
                var validation = new PlaceNameValidator();
                var result = validation.Validate(vm.SearchText, CultureInfo.CurrentCulture);

                return result.IsValid
                    ? ValidationResult.Success
                    : new ValidationResult(result.ErrorContent.ToString(), [nameof(SearchText)]);
            }
            else return new ValidationResult(Resources.Resources.EmptyInputMessage, [nameof(SearchText)]);
        }

        partial void OnSelectedSuggestionChanged(SearchView value)
        {
            if(value != null)
               SearchText = value.Title;

            SuggestionChosen?.Invoke(this, value);
        }

        #endregion
    }
}
