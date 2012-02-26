﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace JSONLevelConverter
{
    internal class MultiFileIndexer<T> : IEnumerable<T>
    {
        private List<List<T>> filedata = new List<List<T>>();
        private List<int> fileoffs = new List<int>();
        private List<bool> fixedoff = new List<bool>();

        public void AddFile(List<T> data, int offset)
        {
            fixedoff.Add(offset != -1);
            if (offset == -1)
                offset = Count;
            filedata.Add(data);
            fileoffs.Add(offset);
        }

        private int GetContainingFile(int index)
        {
            for (int i = filedata.Count - 1; i >= 0; i--)
            {
                if (index >= fileoffs[i] && index - fileoffs[i] < filedata[i].Count)
                    return i;
            }
            return -1;
        }

        public T this[int index]
        {
            get
            {
                int i = GetContainingFile(index);
                if (i == -1) return default(T);
                return filedata[i][index - fileoffs[i]];
            }
            set
            {
                int i = GetContainingFile(index);
                filedata[i][index - fileoffs[i]] = value;
            }
        }

        public void Add(T item)
        {
            filedata[GetContainingFile(Count - 1)].Add(item);
        }

        public void InsertBefore(int index, T insertItem)
        {
            int i = GetContainingFile(index);
            filedata[i].Insert(index - fileoffs[i], insertItem);
            for (i++; i < FileCount; i++)
                if (!fixedoff[i])
                    fileoffs[i]++;
        }

        public void InsertAfter(int index, T insertItem)
        {
            int i = GetContainingFile(index);
            filedata[i].Insert(index - fileoffs[i] + 1, insertItem);
            for (i++; i < FileCount; i++)
                if (!fixedoff[i])
                    fileoffs[i]++;
        }

        public void Remove(T item)
        {
            int i = GetContainingFile(ToList().IndexOf(item));
            filedata[i].Remove(item);
            for (i++; i < FileCount; i++)
                if (!fixedoff[i])
                    fileoffs[i]--;
        }

        public void RemoveAt(int index)
        {
            int i = GetContainingFile(index);
            filedata[i].RemoveAt(index - fileoffs[i]);
            for (i++; i < FileCount; i++)
                if (!fixedoff[i])
                    fileoffs[i]--;
        }

        public void Clear()
        {
            filedata.Clear();
            fileoffs.Clear();
            fixedoff.Clear();
        }

        public int Count
        {
            get
            {
                int maxind = 0;
                for (int i = 0; i < filedata.Count; i++)
                    maxind = Math.Max(fileoffs[i] + filedata[i].Count, maxind);
                return maxind;
            }
        }

        public int FileCount { get { return filedata.Count; } }

        public List<T> ToList()
        {
            List<T> res = new List<T>();
            foreach (T item in this)
            {
                res.Add(item);
            }
            return res;
        }

        public T[] ToArray()
        {
            return ToList().ToArray();
        }

        public ReadOnlyCollection<T> GetFile(int file)
        {
            return new ReadOnlyCollection<T>(filedata[file]);
        }
        
        public ReadOnlyCollection<ReadOnlyCollection<T>> GetFiles()
        {
            List<ReadOnlyCollection<T>> files = new List<ReadOnlyCollection<T>>();
            foreach (List<T> item in filedata)
                files.Add(new ReadOnlyCollection<T>(item));
            return new ReadOnlyCollection<ReadOnlyCollection<T>>(files);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new MultiFileEnumerator<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new MultiFileEnumerator<T>(this);
        }

        internal class MultiFileEnumerator<U> : IEnumerator<U>
        {
            private int current = -1;
            private MultiFileIndexer<U> indexer;

            public MultiFileEnumerator(MultiFileIndexer<U> indexer)
            {
                this.indexer = indexer;
            }

            U IEnumerator<U>.Current
            {
                get { return indexer[current]; }
            }

            void IDisposable.Dispose()
            {

            }

            object IEnumerator.Current
            {
                get { return indexer[current]; }
            }

            bool IEnumerator.MoveNext()
            {
                current++;
                return current < indexer.Count;
            }

            void IEnumerator.Reset()
            {
                current = -1;
            }
        }
    }
}