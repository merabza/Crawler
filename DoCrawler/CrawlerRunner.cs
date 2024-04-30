using System;
using System.Collections.Generic;
using System.Linq;
using CrawlerDb.Models;
using DoCrawler.Domain;
using DoCrawler.Models;
using LibCrawlerRepositories;
using LibDataInput;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace DoCrawler;

public sealed class CrawlerRunner
{
    private readonly ILogger _logger;
    private readonly CrawlerParameters _par;
    private readonly ParseOnePageParameters _parseOnePageParameters;
    private readonly ICrawlerRepository _repository;
    private readonly TaskModel? _task;
    private readonly string? _taskName;

    public CrawlerRunner(ILogger logger, ICrawlerRepository repository, CrawlerParameters par,
        ParseOnePageParameters parseOnePageParameters, string taskName,
        TaskModel task)
    {
        _logger = logger;
        _repository = repository;
        _par = par;
        _parseOnePageParameters = parseOnePageParameters;
        _taskName = taskName;
        _task = task;
    }

    public CrawlerRunner(ILogger logger, ICrawlerRepository repository, CrawlerParameters par,
        ParseOnePageParameters parseOnePageParameters)
    {
        _logger = logger;
        _repository = repository;
        _par = par;
        _parseOnePageParameters = parseOnePageParameters;
        _taskName = null;
        _task = null;
    }

    public void Run(Batch? startBatch = null)
    {
        try
        {
            var par = ParseOnePageParameters.Create(_par);
            if (par is null)
            {
                StShared.WriteErrorLine("ParseOnePageParameters does not created", true);
                return;
            }

            var batch = startBatch ?? GetBatchByTaskName();
            if (batch is null)
            {
                StShared.WriteErrorLine("batch is null", true);
                return;
            }

            var batchPart = _repository.GetOpenedBatchPart(batch.BatchId);
            BatchPartRunner? batchPartRunner = null;
            while (true)
            {
                var createNewPart = false;
                if (batchPart == null)
                {
                    createNewPart = batch.AutoCreateNextPart;
                    if (!batch.AutoCreateNextPart)
                    {
                        if (!Inputer.InputBool($"Opened part not found for bath {batch.BatchName}, Create new?",
                                true, false))
                            return;
                        createNewPart = true;
                    }
                }

                if (createNewPart)
                {
                    batchPart = _repository.TryCreateNewPart(batch.BatchId);
                    _repository.SaveChanges();


                    batchPartRunner =
                        new BatchPartRunner(_logger, _repository, _par, _parseOnePageParameters, batchPart);

                    batchPartRunner.InitBachPart(_task?.StartPoints ?? new List<string>(), batch);
                }
                else if (batchPart is not null)
                {
                    batchPartRunner =
                        new BatchPartRunner(_logger, _repository, _par, _parseOnePageParameters, batchPart);
                }

                if (batchPartRunner is null)
                {
                    _logger.LogError("batchPartRunner is null");
                    return;
                }

                batchPartRunner.RunBatchPart();


                batchPart = null;
            }
        }
        catch (DataInputEscapeException)
        {
            Console.WriteLine();
            Console.WriteLine("Escape... ");
            StShared.Pause();
        }
        catch (Exception e)
        {
            StShared.WriteException(e, true);
            throw;
        }
    }


    private Batch? GetBatchByTaskName()
    {
        if (_taskName == null || _task == null || _task.StartPoints.Count == 0)
        {
            _logger.LogError("Not enough Information About Task");
            return null;
        }

        //მოიძებნოს ბაზაში Batch სახელით _taskName
        //თუ არ არსებობს Batch სახელით _taskName, შეიქმნას IsOpen=false, AutoCreateNextPart=false
        var batch = _repository.GetBatchByName(_taskName);
        if (batch == null)
        {
            batch = _repository.CreateBatch(new Batch(_taskName, false, false));
            _repository.SaveChanges();
        }


        //მოხდეს _task.StartPoints-ების განხილვა. თითოეულისათვის:
        //გამოიყოს საწყისი მისამართიდან ჰოსტი და სქემა,
        //შემოწმდეს და თუ არ არსებობს ბაზაში ასეთი ჰოსტი ან სქემა, დარეგისტრირდეს თითოეული.
        //ამ სქემისა და ჰოსტის წყვილისთვის შემოწმდეს არის თუ არა დარეგისტრირებული HostByBatch ცხრილში
        //თუ არ არის დარეგისტრირებული, დარეგისტრირდეს.
        foreach (var myUri in _task.StartPoints.Select(UriFabric.GetUri).Where(myUri => myUri != null))
            _repository.AddHostNamesByBatch(batch, myUri!.Scheme, myUri.Host);

        _repository.SaveChanges();

        _logger.LogInformation("Crawling for batch {0}", batch.BatchName);

        return batch;
    }

    public bool RunOnePage(string strUrl)
    {
        try
        {
            var batch = GetBatchByTaskName();
            if (batch is null)
            {
                StShared.WriteErrorLine("batch is null", true);
                return false;
            }

            var batchPart = _repository.GetOpenedBatchPart(batch.BatchId);
            if (batchPart is null)
            {
                StShared.WriteErrorLine("batchPart is null", true);
                return false;
            }


            BatchPartRunner batchPartRunner = new(_logger, _repository, _par, _parseOnePageParameters, batchPart);

            if (!batchPartRunner.DoOnePage(strUrl, batchPart))
                return false;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred executing {0} for {1}", nameof(RunOnePage), strUrl);
            throw;
        }

        return true;
    }
}