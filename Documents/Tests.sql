SELECT TT.ttId, TT.ttKey, TT.ttName, COUNT(trmId)
FROM terms T
  INNER JOIN termTypes TT ON T.termTypeId = TT.ttId
  GROUP BY TT.ttId, TT.ttKey, TT.ttName

SELECT TT.ttId, TT.ttKey, TT.ttName, COUNT(tbuId)
FROM termsByUrls TBU
  INNER JOIN terms T ON TBU.termId = T.trmId
  INNER JOIN termTypes TT ON T.termTypeId = TT.ttId
GROUP BY TT.ttId, TT.ttKey, TT.ttName

SELECT COUNT(caId)
FROM contentAnalyses
WHERE batchPartId = 1

SELECT bpId, batchId, created, finished, DATEDIFF(HOUR, finished, created)
FROM batchParts


SELECT urlId, urlName, hostId, extensionId, schemeId, urlHashCode, isSiteMap
FROM urls
WHERE hostId = 1

SELECT caId, batchPartId, urlId, responseStatusCode, finish
FROM contentAnalyses
ORDER BY finish desc

SELECT trmId, termText, termTypeId
FROM terms
ORDER BY termText desc