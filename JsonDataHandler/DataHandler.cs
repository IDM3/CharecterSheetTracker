using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

using Newtonsoft.Json;

namespace JsonDataHandler
{
    public class DataHandler<T> : IDisposable where T : IIndexEntity, new()
    {
        private static BlockingCollection<string> existingHandlers;
        public void Dispose()
        {
            if(!existingHandlers.TryTake(out _FileLocation))
            {
                throw new Exception("Couldn't acccess blocking collection.");
            }
        }

        private string _FileLocation;
        private string _DefaultData = string.Empty;

        public DataHandler(string folderPath)
        {
            _FileLocation = folderPath + new T().GetType().Name;
            if(existingHandlers == null)
            {
                existingHandlers = new BlockingCollection<string>();
            }
            if (existingHandlers.Contains(_FileLocation))
            {
                throw new Exception("Please use existing " + _FileLocation + " data handler");
            }
            else
            {
                if(!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                existingHandlers.Add(_FileLocation);
            }
        }

        public DataHandler(string folderPath, string defaultData)
        {
            _DefaultData = defaultData;
            _FileLocation = folderPath + new T().GetType().Name;
            if (existingHandlers == null)
            {
                existingHandlers = new BlockingCollection<string>();
            }
            if (existingHandlers.Contains(_FileLocation))
            {
                throw new Exception("Please use existing " + _FileLocation + " data handler");
            }
            else
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                existingHandlers.Add(_FileLocation);
            }
        }

        public void Delete(T objectToDelete)
        {
            Delete(objectToDelete.Id);
        }

        public void Delete(int id)
        {
            List<T> allFiles = GetAll();
            if(IdExists(id))
            {
                allFiles.RemoveAt(allFiles.FindIndex(x => x.Id == id));
                string fileData = JsonConvert.SerializeObject(allFiles);
                try
                {
                    File.WriteAllText(_FileLocation, fileData);
                }
                catch (InvalidOperationException e)
                {
#if DEBUG
                    throw e;
#endif
                }
                catch (ArgumentException e)
                {
#if DEBUG
                    throw e;
#endif
                }  
            }
            else
            {
                //already gone
            }
        }

        public void Save(T objectToInput)
        {
            List<T> allFiles = GetAll();
            Predicate<T> isItem = entity => entity.Id == objectToInput.Id && objectToInput.Id != 0;
            bool itemIdIsNotDefault = objectToInput.Id != 0;
            bool fileContainsItemId = allFiles.Any(x => isItem(x));
            if (itemIdIsNotDefault && fileContainsItemId)
            {
                allFiles[allFiles.FindIndex(isItem)] = objectToInput;
            }
            else
            {
                if(allFiles.Any())
                {
                    objectToInput.Id = allFiles.Max(x => x.Id) + 1;
                }
                else
                {
                    objectToInput.Id = 1;
                }
                allFiles.Add(objectToInput);
            }
            string fileData = JsonConvert.SerializeObject(allFiles);
            try
            {
                File.WriteAllText(_FileLocation, fileData);
            }
            catch (InvalidOperationException e)
            {
#if DEBUG
                throw e;
#endif
            }
            catch (ArgumentException e)
            {
#if DEBUG
                throw e;
#endif
            }  
        }

        private List<T> _Values;
        public List<T> GetAll()
        {
            List<T> items;
            if(_Values == null)
            {
                items = new List<T>();
                
                if(File.Exists(_FileLocation) || !string.IsNullOrWhiteSpace(_DefaultData))
                {
                    string rawFile;
                    if(File.Exists(_FileLocation))
                    {
                        rawFile = File.ReadAllText(_FileLocation);
                    }
                    else
                    {
                        rawFile = _DefaultData;
                    }
                    if(string.IsNullOrWhiteSpace(rawFile))
                    {
                        //file is empty
                    }
                    else
                    {
                        try
                        {
                            items = JsonConvert.DeserializeObject<List<T>>(rawFile);
                            _Values = items;
                        }
                        catch (InvalidOperationException e)
                        {
#if DEBUG
                            throw e;
#endif
                        }
                        catch (ArgumentException e)
                        {
#if DEBUG
                            throw e;
#endif
                        } 
                    }
                }
            }
            else
            {
                items = _Values;
            }
            return items;
        }

        public T GetById(int id)
        {
            T item = GetAll().FirstOrDefault(entity => entity.Id == id);
            if(item == null)
            {
                item = new T();
            }
            return item;
        }

        public bool IdExists(int id)
        {
            bool exists = GetAll().Any(entity => entity.Id == id);
            return exists;
        }
    }
}
