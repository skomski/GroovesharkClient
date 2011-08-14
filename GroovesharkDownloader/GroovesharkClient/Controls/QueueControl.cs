using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GroovesharkAPI.Types.Songs;

namespace GroovesharkPlayer.Controls
{
    public partial class QueueControl : UserControl
    {
        public QueueControl()
        {
            InitializeComponent();

            AudioPlayer.Instance.Songs.CollectionChanged += SongsCollectionChanged;
        }

        void SongsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    songsControl.AddRange(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    songsControl.RemoveRange(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move: 
                    songsControl.RemoveRange(e.OldItems);
                    songsControl.AddRange(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if(AudioPlayer.Instance.CurrentSong != null)
            {
                songsControl.Mark(AudioPlayer.Instance.CurrentSong);
            }
        }
    }
}
