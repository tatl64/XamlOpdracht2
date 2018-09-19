﻿using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using MVVM.Views;
using SqliteOpdrachten.Models;

namespace MVVM.ViewModels
{
    public  class ContactBookViewModel : BaseViewModel
    {

        private ContactViewModel _selectedContact;
        private IContactStore _contactStore;
        private IPageService _pageService;
        private bool _isDataLoaded;


        public ObservableCollection<ContactViewModel> Contacts
        { get; private set; }
            = new ObservableCollection<ContactViewModel>();

        public ContactViewModel SelectedContact
        {
            get { return _selectedContact; }
            set { SetValue(ref _selectedContact, value); }
        }

        public ICommand LoadDataCommand { get; private set; }
        public ICommand AddContactCommand { get; private set; }
        public ICommand SelectContactCommand { get; private set; }
        public ICommand DeleteContactCommand { get; private set; }

        public ContactBookViewModel(IContactStore contactStore, IPageService pageService)
        {
            _contactStore = contactStore;
            _pageService = pageService;

            LoadDataCommand = new Command(async () => await LoadData());
            AddContactCommand = new Command(async () => await AddContact());
            SelectContactCommand = new Command<ContactViewModel>(async c => await SelectContact(c));
            DeleteContactCommand = new Command<ContactViewModel>(async c => await DeleteContact(c));
         }

        private async Task LoadData()
        {
            if (_isDataLoaded)
                return;

            _isDataLoaded = true;

            var contacts = await _contactStore.GetContactsAsync();

            foreach (var c in contacts)
                Contacts.Add(new ContactViewModel(c));
        }

        private async Task AddContact()
        {
            var viewModel = new ContactDetailViewModel(new ContactViewModel(), _contactStore, _pageService);

            viewModel.ContactAdded += (source, contact) =>
            {
                Contacts.Add(new ContactViewModel(contact));
            };

            await _pageService.PopModalAsync(new ContactDetails(viewModel));
        }

        private async Task SelectContact(ContactViewModel contact)
        {
            if (contact == null)
                return;

            SelectedContact = null;

            var viewModel = new ContactDetailViewModel(contact, _contactStore, _pageService);
            viewModel.ContactUpdated += (source, updatedContact) =>
            {
                contact.Id = updatedContact.Id;
                contact.FirstName = updatedContact.FirstName;
                contact.LastName = updatedContact.LastName;
                contact.Phone = updatedContact.Phone;
                contact.Email = updatedContact.Email;
                contact.IsBlocked = updatedContact.IsBlocked;
            };

            await _pageService.PopModalAsync(new ContactDetails(viewModel));
        }

        private async Task DeleteContact(ContactViewModel contactViewModel)
        {
            var a = "";
            if (await _pageService.DisplayAlert("Warning", $"Are you sure you want to delete {contactViewModel.FullName}?", "Yes", "No"))
            {
                Contacts.Remove(contactViewModel);


                var contact = await _contactStore.GetContact(contactViewModel.Id);
                await _contactStore.DeleteContact(contact);
            }
        }
    }
}