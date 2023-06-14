using System;
using System.Collections.Concurrent;
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

    //public readonly ConcurrentQueue<IForBase> BaseCommandsqueue = new ConcurrentQueue<IForBase>();
    public readonly ConcurrentQueue<UrlModel> UrlsQueue = new();

    private int _lastStateId;
    //public readonly ConcurrentDictionary<string, GetPagesFromHostState> CurrentHosts = new ConcurrentDictionary<string, GetPagesFromHostState>();
    //public readonly SaveToBaseState SaveToBaseState = new SaveToBaseState();
    //public readonly Collector Collector = new Collector();
    //public Dictionary<string, List<string>> ProhibitedQueries;
    //public List<string> ProhibitedContentTypes;
    //public List<string> DomainRootNamesWithDot;

    //public string LastUsedCommand { private get; set; }
    //public bool BatchIdChangeDenide { get; set; }
    //public int BatchId { get; set; }

    private ProcData( /*bool init = true*/)
    {
        //if (init)
        //  LoadFrombase();
    }

    //private int _currentId;
    //public HostModel GetHostByName(string hostName)
    //{
    //  if (!_hostsCache.TryAdd(currentId, DateTime.Now))
    //    return true;
    //  while (_hostNamesCache.Count > MaxCacheRecordCount)
    //    _hostNamesCache.TryRemove(_hostNamesCache.OrderBy(m => m.Value).SingleOrDefault());
    //  return false;
    //}

    public UrlModel? GetUrlByHashCode(int hashCode)
    {
        return _urlCache.ContainsKey(hashCode) ? _urlCache[hashCode] : null;
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
        return _schemesCache.TryGetValue(schemeName, out var scheme) ? scheme : null;
    }

    public void AddScheme(SchemeModel scheme)
    {
        _schemesCache.TryAdd(scheme.SchName, scheme);
    }

    public ExtensionModel? GetExtensionByName(string extensionName)
    {
        return _extensionsCache.TryGetValue(extensionName, out var extension) ? extension : null;
    }

    public void AddExtension(ExtensionModel extension)
    {
        _extensionsCache.TryAdd(extension.ExtName, extension);
    }

    public HostModel? GetHostByName(string hostName)
    {
        return _hostsCache.TryGetValue(hostName, out var host) ? host : null;
    }

    public void AddHost(HostModel host)
    {
        _hostsCache.TryAdd(host.HostName, host);
    }

    internal TermType? GetTermTypeByKey(string termTypeName)
    {
        return _termTypesCache.TryGetValue(termTypeName, out var termType) ? termType : null;
    }

    public void AddTermType(TermType termTypeInBase)
    {
        _termTypesCache.TryAdd(termTypeInBase.TtKey, termTypeInBase);
    }

    public Term? GetTermByName(string termText)
    {
        return _termCache.TryGetValue(termText, out var term) ? term : null;
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