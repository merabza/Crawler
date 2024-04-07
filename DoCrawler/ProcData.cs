using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CrawlerDb.Models;

namespace DoCrawler;

public sealed class ProcData : IDisposable
{
    private const int MaxCacheRecordCount = 10000;
    private readonly ConcurrentDictionary<string, ExtensionModel> _extensionsCache = new();


    private readonly ConcurrentDictionary<string, HostModel> _hostsCache = new();
    private readonly ConcurrentDictionary<string, SchemeModel> _schemesCache = new();
    private readonly ConcurrentDictionary<string, Term> _termCache = new();
    private readonly ConcurrentDictionary<string, TermType> _termTypesCache = new();
    private readonly ConcurrentDictionary<int, UrlModel> _urlCache = new();

    public readonly ConcurrentQueue<UrlModel> UrlsQueue = new();

    private int _lastStateId;

    private ProcData()
    {

    }

    public UrlModel? GetUrlByHashCode(int hashCode)
    {
        return _urlCache.GetValueOrDefault(hashCode);
    }

    public bool NeedsToReduceCache()
    {
        return _urlCache.Count > MaxCacheRecordCount || _termCache.Count > MaxCacheRecordCount;
    }

    public void ReduceCache()
    {
        if (_urlCache.Count > MaxCacheRecordCount)
            _urlCache.Clear();
        if (_schemesCache.Count > MaxCacheRecordCount)
            _schemesCache.Clear();
        if (_extensionsCache.Count > MaxCacheRecordCount)
            _extensionsCache.Clear();
        if (_hostsCache.Count > MaxCacheRecordCount)
            _hostsCache.Clear();
        if (_termTypesCache.Count > MaxCacheRecordCount)
            _termTypesCache.Clear();
        if (_termCache.Count > MaxCacheRecordCount)
            _termCache.Clear();
        GC.Collect();
    }

    public void AddUrl(UrlModel url)
    {
        _urlCache.AddOrUpdate(url.UrlHashCode, url, (_, _) => url);
    }

    internal int GetNewStateId()
    {
        lock (this)
        {
            _lastStateId++;
            return _lastStateId;
        }
    }


    public SchemeModel? GetSchemeByName(string schemeName)
    {
        return _schemesCache.GetValueOrDefault(schemeName);
    }

    public void AddScheme(SchemeModel scheme)
    {
        _schemesCache.TryAdd(scheme.SchName, scheme);
    }

    public ExtensionModel? GetExtensionByName(string extensionName)
    {
        return _extensionsCache.GetValueOrDefault(extensionName);
    }

    public void AddExtension(ExtensionModel extension)
    {
        _extensionsCache.TryAdd(extension.ExtName, extension);
    }

    public HostModel? GetHostByName(string hostName)
    {
        return _hostsCache.GetValueOrDefault(hostName);
    }

    public void AddHost(HostModel host)
    {
        _hostsCache.TryAdd(host.HostName, host);
    }

    internal TermType? GetTermTypeByKey(string termTypeName)
    {
        return _termTypesCache.GetValueOrDefault(termTypeName);
    }

    public void AddTermType(TermType termTypeInBase)
    {
        _termTypesCache.TryAdd(termTypeInBase.TtKey, termTypeInBase);
    }

    public Term? GetTermByName(string termText)
    {
        return _termCache.GetValueOrDefault(termText);
    }

    public void AddTerm(Term term)
    {
        _termCache.TryAdd(term.TermText, term);
    }

    #region Singletone

    private static ProcData? _instance;
    private static readonly object SyncRoot = new();

    public static ProcData Instance
    {
        get
        {
            if (_instance != null)
                return _instance;
            lock (SyncRoot) //thread safe singleton
            {
                _instance ??= new ProcData();
            }

            return _instance;
        }
    }

    public static void NewSession()
    {
        if (_instance == null) return;
        _instance.Dispose();
        _instance = null;
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    //Every unsealed root IDisposable type must provide its own protected virtual void Dispose(bool) method. 
    //Dispose() should call Dipose(true) and Finalize should call Dispose(false). 
    //If you are creating an unsealed root IDisposable type, you must define Dispose(bool) and call it
    ~ProcData()
    {
        // Finalizer calls Dispose(false)
        Dispose(false);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            // free managed resources
        }
        // free native resources if there are any.
        //if (nativeResource != IntPtr.Zero)
        //{
        //  Marshal.FreeHGlobal(nativeResource);
        //  nativeResource = IntPtr.Zero;
        //}
    }

    #endregion
}