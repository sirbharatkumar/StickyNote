using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.IO;
using System.Windows.Browser;

namespace Liquid.Components
{
    public delegate void UploadEventHandler(object sender, UploadEventArgs e);

    public partial class Uploader
    {
        #region Private Properties

        private UploaderItem _current = null;
        private long _totalUploadSize = 0;
        private long _totalUploaded = 0;
        private int _itemsUploaded = 0;
        private int _itemsTotal = 0;
        private long _position = 0;
        private DateTime _startTime = DateTime.Now;
        private TimeSpan _duration = new TimeSpan();
        private long _count = 0;
        private bool _aborted = false;
        private bool _isPaused = false;
        private bool _uploadPaused = false;
        private string _serverResponse = string.Empty;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the list of UploadItem objects
        /// </summary>
        public ObservableCollection<UploaderItem> Items = new ObservableCollection<UploaderItem>();

        /// <summary>
        /// Gets the current item being uploaded
        /// </summary>
        public UploaderItem Current { get { return _current; } }

        /// <summary>
        /// Gets or sets the progress completion value
        /// </summary>
        public double Complete { get; set; }

        /// <summary>
        /// Gets or sets the web service url to handle the upload on the server-side
        /// </summary>
        public string WebserviceURL { get; set; }

        /// <summary>
        /// Gets or sets the name of the web service that will be called during the upload
        /// </summary>
        public string WebserviceMethod { get; set; }

        /// <summary>
        /// The maximum size of a single data transmission
        /// </summary>
        public int PacketSize { get; set; }

        /// <summary>
        /// Gets or sets the state of the upload
        /// </summary>
        public bool Busy { get; set; }

        /// <summary>
        /// Gets the total size in bytes of the data to upload
        /// </summary>
        public long TotalUploadSize { get { return _totalUploadSize; } }

        /// <summary>
        /// Gets the total amount of bytes transmitted
        /// </summary>
        public long TotalUploaded { get { return _totalUploaded; } }

        /// <summary>
        /// Gets the total number of items to upload
        /// </summary>
        public int ItemsUploaded { get { return _itemsUploaded; } }

        /// <summary>
        /// Gets the total number of items transmitted
        /// </summary>
        public int ItemsTotal { get { return _itemsTotal; } }

        /// <summary>
        /// Gets the start time of the first transmission
        /// </summary>
        public DateTime StartTime { get { return _startTime; } }

        /// <summary>
        /// Gets the duration of the transmission
        /// </summary>
        public TimeSpan Duration { get { return _duration; } }

        /// <summary>
        /// Gets or sets whether existing files will be overwritten
        /// </summary>
        public bool Overwrite { get; set; }

        /// <summary>
        /// Gets or sets whether the upload is paused
        /// </summary>
        public bool IsPaused
        {
            get { return _isPaused; }
            set
            {
                if (!_isPaused && value && _uploadPaused)
                {
                    // Resuming
                    Continue();
                    _uploadPaused = false;
                }
                _isPaused = value;
            }
        }

        /// <summary>
        /// Gets the response text for the last packet transmission
        /// </summary>
        public string ServerResponse
        {
            get { return _serverResponse; }
        }

        #endregion

        #region Public Events

        public event UploadEventHandler UploadedItem;
        public event UploadEventHandler UploadFinished;
        public event UploadEventHandler UploadProgressChange;

        #endregion

        #region Constructor

        public Uploader(string webserviceURL)
            : this(webserviceURL, "Upload")
        {
        }

        public Uploader(string webserviceURL, string webserviceMethod)
        {
            WebserviceURL = webserviceURL;
            WebserviceMethod = webserviceMethod;
            PacketSize = 32768;

            Items.CollectionChanged += new NotifyCollectionChangedEventHandler(Items_CollectionChanged);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Resets the uploader ready for a new batch of upload data
        /// </summary>
        public void Reset()
        {
            _startTime = DateTime.Now;
            _duration = new TimeSpan();
            _totalUploadSize = 0;
            _totalUploaded = 0;
            _itemsTotal = 0;
            _itemsUploaded = 0;
            _aborted = false;
            Complete = 0;

            Items.Clear();
        }

        /// <summary>
        /// Uploads a list of files provided by a File Open dialog
        /// </summary>
        /// <param name="id">An identifier</param>
        /// <param name="files">A list of files to upload</param>
        /// <param name="targetPath">The target path on the server for the files</param>
        /// <param name="overwrite">Indicates whether the file should be over-written on the server</param>
        /// <param name="tag">A generic value to be transmitted with the data</param>
        public void UploadFiles(string id, IEnumerable<FileInfo> files, string targetPath, bool overwrite, string tag)
        {
            Reset();
            Overwrite = overwrite;

            foreach (FileInfo info in files)
            {
                Stream temp = info.OpenRead();
                Items.Add(new UploaderItem(id, temp, info.Name, targetPath, string.Empty, temp.Length, tag));
            }
        }

        /// <summary>
        /// Uploads a single file
        /// </summary>
        /// <param name="id">An identifier</param>
        /// <param name="file">A single file object</param>
        /// <param name="targetPath">The target path on the server for the file</param>
        /// <param name="overwrite">Indicates whether the file should be over-written on the server</param>
        /// <param name="tag">A generic value to be transmitted with the data</param>
        public void UploadFile(string id, FileInfo file, string targetPath, bool overwrite, string tag)
        {
            UploadFile(id, file, targetPath, string.Empty, overwrite, tag);
        }

        /// <summary>
        /// Uploads a single file
        /// </summary>
        /// <param name="id">An identifier</param>
        /// <param name="file">A single file object</param>
        /// <param name="targetPath">The target path on the server for the file</param>
        /// <param name="targetFilename">The target filename</param>
        /// <param name="overwrite">Indicates whether the file should be over-written on the server</param>
        /// <param name="tag">A generic value to be transmitted with the data</param>
        public void UploadFile(string id, FileInfo file, string targetPath, string targetFilename, bool overwrite, string tag)
        {
            Stream temp = file.OpenRead();
            UploadData(id, temp, file.Name, targetPath, targetFilename, overwrite, tag);
        }

        /// <summary>
        /// Uploads a stream of data
        /// </summary>
        /// <param name="id">An identifier</param>
        /// <param name="stream">A data stream</param>
        /// <param name="filename">The filename for the stream</param>
        /// <param name="targetPath">The target path on the server for the file</param>
        /// <param name="targetFilename">The target filename</param>
        /// <param name="overwrite">Indicates whether the file should be over-written on the server</param>
        /// <param name="tag">A generic value to be transmitted with the data</param>
        public void UploadData(string id, Stream stream, string filename, string targetPath, string targetFilename, bool overwrite, string tag)
        {
            Items.Clear();
            Overwrite = overwrite;

            Items.Add(new UploaderItem(id, stream, filename, targetPath, targetFilename, stream.Length, tag));
        }

        /// <summary>
        /// Aborts the current and all waiting uploads
        /// </summary>
        public void Abort()
        {
            if (Busy)
            {
                _aborted = true;
            }
            else
            {
                Reset();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Start the upload by first checking for an item in the collection and starting the transmission
        /// </summary>
        private void BeginUpload()
        {
            if (Items.Count > 0)
            {
                _current = Items[0];
            }

            if (_current != null)
            {
                Busy = true;
                _position = 0;

                Continue();
            }
            else
            {
                Complete = 100;

                if (Busy)
                {
                    RaiseUploadFinished(this, new UploadEventArgs("", Complete, _duration));
                }

                Busy = false;
            }
        }

        /// <summary>
        /// Uploads the next packet of data for the current upload
        /// </summary>
        /// <returns>Number of bytes uploaded</returns>
        private void Continue()
        {
            bool final = false;

            _count = PacketSize;

            if (_current != null)
            {
                _duration = DateTime.Now - StartTime;

                if (_position < _current.Length)
                {
                    if ((_position + _count) >= _current.Length)
                    {
                        _count = _current.Length - _position;
                        final = true;
                    }

                    Transmit(_position, _count, final);
                }
            }
        }

        /// <summary>
        /// Transmits a block of data
        /// </summary>
        /// <param name="startIndex">Start index within the byte array</param>
        /// <param name="count">The number of bytes to send</param>
        /// <param name="final">Indicates whether this is the final packet</param>
        private void Transmit(long startIndex, long count, bool final)
        {
            string content = string.Empty;
            string mode = "new";
            string url = WebserviceURL + "/" + WebserviceMethod;
            Comms helper = null;

            if (_current.DataStream != null)
            {
                byte[] buffer = new byte[count];

                _current.DataStream.Read(buffer, 0, (int)count);
                content = Convert.ToBase64String(buffer);
            }
            else
            {
                content = Convert.ToBase64String(_current.Data, (int)startIndex, (int)count);
            }

            if (startIndex > 0)
            {
                mode = "append";
            }

            if (_current.TargetFileName.Length > 0)
            {
                helper = new Comms(new Uri(url), "POST", false,
                    new KeyValuePair<string, string>("id", HttpUtility.UrlEncode(_current.ID)),
                    new KeyValuePair<string, string>("mode", HttpUtility.UrlEncode(mode)),
                    new KeyValuePair<string, string>("path", HttpUtility.UrlEncode(_current.TargetPath)),
                    new KeyValuePair<string, string>("name", HttpUtility.UrlEncode(_current.FileName)),
                    new KeyValuePair<string, string>("targetname", HttpUtility.UrlEncode(_current.TargetFileName)),
                    new KeyValuePair<string, string>("filedata", HttpUtility.UrlEncode(content)),
                    new KeyValuePair<string, string>("overwrite", Overwrite.ToString()),
                    new KeyValuePair<string, string>("tag", HttpUtility.UrlEncode(_current.Tag)),
                    new KeyValuePair<string, string>("final", final.ToString()));
            }
            else
            {
                helper = new Comms(new Uri(url), "POST", false,
                    new KeyValuePair<string, string>("id", HttpUtility.UrlEncode(_current.ID)),
                    new KeyValuePair<string, string>("mode", HttpUtility.UrlEncode(mode)),
                    new KeyValuePair<string, string>("path", HttpUtility.UrlEncode(_current.TargetPath)),
                    new KeyValuePair<string, string>("name", HttpUtility.UrlEncode(_current.FileName)),
                    new KeyValuePair<string, string>("filedata", HttpUtility.UrlEncode(content)),
                    new KeyValuePair<string, string>("overwrite", Overwrite.ToString()),
                    new KeyValuePair<string, string>("tag", HttpUtility.UrlEncode(_current.Tag)),
                    new KeyValuePair<string, string>("final", final.ToString()));
            }
            helper.ResponseComplete += new HttpResponseCompleteEventHandler(this.CommandComplete);
            helper.Execute();
        }

        /// <summary>
        /// This is called when an item has been uploaded
        /// </summary>
        private void UploadItemComplete()
        {
            UploaderItem temp;

            if (_current.DataStream != null)
            {
                _current.DataStream.Close();
            }

            temp = _current;
            _current = null;

            RaiseUploadProgressChange(this, new UploadEventArgs(temp.FileName, Complete, _duration));
            RaiseUploadedItem(this, new UploadEventArgs(temp.FileName, Complete, _duration));
            _itemsUploaded++;

            Items.Remove(temp);

            BeginUpload();
        }

        /// <summary>
        /// This is called to abort the current item upload
        /// </summary>
        private void AbortUploadItem()
        {
            long temp = 0;

            UploadItemComplete();

            foreach (UploaderItem item in Items)
            {
                temp += item.Length;
            }

            _totalUploaded = _totalUploadSize - temp;
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// This is called when a packet of data has been sucessfully received on the server
        /// </summary>
        /// <param name="e">Event arguments</param>
        private void CommandComplete(HttpResponseCompleteEventArgs e)
        {
            _serverResponse = e.Response;

            if (!_aborted && e.Response.Contains("ok"))
            {
                _totalUploaded += _count;
                _position += _count;

                Complete = ((double)_totalUploaded / (double)_totalUploadSize) * 100;
                RaiseUploadProgressChange(this, new UploadEventArgs(_current.FileName, Complete, _duration));

                if (_position < _current.Length)
                {
                    if (!IsPaused)
                    {
                        Continue();
                    }
                    else
                    {
                        _uploadPaused = true;
                    }
                }
                else
                {
                    UploadItemComplete();
                }
            }
            else
            {
                AbortUploadItem();

                if (_aborted)
                {
                    Reset();
                }
            }
        }

        /// <summary>
        /// This is called when the Items collection is changed
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!_aborted)
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (UploaderItem item in e.NewItems)
                    {
                        _totalUploadSize += item.Length;
                        _itemsTotal++;
                    }
                }

                if (!Busy)
                {
                    BeginUpload();
                }
            }
        }

        #endregion

        #region Event Raising

        /// <summary>
        /// Generates a UploadedItem event when an item has been fully uploaded
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaiseUploadedItem(object sender, UploadEventArgs args)
        {
            if (UploadedItem != null)
            {
                UploadedItem(sender, args);
            }
        }

        /// <summary>
        /// Generates a UploadFinished event when all uploads are complete
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaiseUploadFinished(object sender, UploadEventArgs args)
        {
            if (UploadFinished != null)
            {
                UploadFinished(sender, args);
            }
        }

        /// <summary>
        /// Generates a UploadProgressChange event when upload progress changes
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaiseUploadProgressChange(object sender, UploadEventArgs args)
        {
            if (UploadProgressChange != null)
            {
                UploadProgressChange(sender, args);
            }
        }

        #endregion
    }

    #region UploadEventArgs

    public partial class UploadEventArgs : EventArgs
    {
        public string Text { get; set; }
        public double Progress { get; set; }
        public TimeSpan Duration { get; set; }

        public UploadEventArgs()
        {
            Text = string.Empty;
            Progress = 0;
            Duration = new TimeSpan();
        }

        public UploadEventArgs(string text, double progress, TimeSpan duration)
        {
            Text = text;
            Progress = progress;
            Duration = duration;
        }
    }

    #endregion

    #region UploaderItem

    public partial class UploaderItem
    {
        public string ID { get; set; }
        public string FileName { get; set; }
        public byte[] Data { get; set; }
        public Stream DataStream { get; set; }
        public string TargetPath { get; set; }
        public string TargetFileName { get; set; }
        public long Length { get; set; }
        public string Tag { get; set; }

        public UploaderItem()
        {
            FileName = string.Empty;
            TargetPath = string.Empty;
            TargetFileName = string.Empty;
        }

        public UploaderItem(byte[] data)
        {
            Data = data;
            Length = data.Length;
        }

        public UploaderItem(Stream datastream, long length)
        {
            DataStream = datastream;
            Length = length;
        }

        public UploaderItem(byte[] data, string filename, string targetpath, string targetFilename)
        {
            Data = data;
            FileName = filename;
            TargetPath = targetpath;
            TargetFileName = targetFilename;
            Length = data.Length;
        }

        public UploaderItem(string id, Stream datastream, string filename, string targetpath, string targetFilename, long length, string tag)
        {
            ID = id;
            DataStream = datastream;
            FileName = filename;
            TargetPath = targetpath;
            TargetFileName = targetFilename;
            Length = length;
            Tag = tag;
        }
    }

    #endregion
}
