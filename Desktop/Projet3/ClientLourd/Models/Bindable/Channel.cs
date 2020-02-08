﻿using System.Collections.ObjectModel;
using System.Linq;

namespace ClientLourd.Models.Bindable
{
    public class Channel : ModelBase
    {
        public Channel()
        {
        }

        public ObservableCollection<User> Members
        {
            get { return _members; }
            set
            {
                if (value != _members)
                {
                    _members = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private ObservableCollection<User> _members;
        
        public string ID
        {
            get { return _id; }
            set
            {
                if (value != _id)
                {
                    _id = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string _id;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string _name;

        public ObservableCollection<Message> Messages
        {
            get { return _messages; }
            set
            {
                if (value != _messages)
                {
                    _messages = new ObservableCollection<Message>(value.OrderBy(m => m.Date).ToList());
                    NotifyPropertyChanged();
                }
            }
        }

        private ObservableCollection<Message> _messages;
    }
}