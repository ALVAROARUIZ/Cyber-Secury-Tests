USE [DiveViewLab];
GO

IF NOT EXISTS (SELECT 1 FROM lab.WallabeeVersions WHERE Version = '0.1.0')
    INSERT lab.WallabeeVersions (Version, MajorVersion, MinorVersion, PatchVersion, Description) VALUES ('0.1.0', 0, 1, 0, N'Initial Wallabee laboratory version');
IF NOT EXISTS (SELECT 1 FROM lab.HoneyBadgerVersions WHERE Version = '0.1.0')
    INSERT lab.HoneyBadgerVersions (Version, MajorVersion, MinorVersion, PatchVersion, Description) VALUES ('0.1.0', 0, 1, 0, N'Initial Honey Badger synthetic-sample generator version');
GO

MERGE lab.IndicatorDefinitions AS target
USING (VALUES
    ('HIGH_ENTROPY', N'High entropy', N'Wallabee', N'File entropy exceeds the configured threshold.', CAST(1.0000 AS DECIMAL(9,4)), N'Entropy'),
    ('HEADER_MISMATCH', N'Header mismatch', N'Wallabee', N'File header conflicts with the declared type.', CAST(1.0000 AS DECIMAL(9,4)), N'Header'),
    ('KNOWN_HASH', N'Known hash', N'Wallabee', N'SHA-256 matches a configured indicator set.', CAST(1.0000 AS DECIMAL(9,4)), N'Hash'),
    ('EXTENSION_MISMATCH', N'Extension mismatch', N'Wallabee', N'File extension conflicts with its detected type.', CAST(0.7500 AS DECIMAL(9,4)), N'Name'),
    ('SIZE_ANOMALY', N'Size anomaly', N'Wallabee', N'File size is unusual for its family.', CAST(0.5000 AS DECIMAL(9,4)), N'Size'),
    ('REPEATED_PATTERN', N'Repeated pattern', N'Wallabee', N'Heuristic repeated pattern observed.', CAST(0.5000 AS DECIMAL(9,4)), N'Pattern'),
    ('SYNTHETIC_ANOMALY', N'Synthetic anomaly', N'HoneyBadger', N'Synthetic generation anomaly observed.', CAST(0.5000 AS DECIMAL(9,4)), N'Synthetic')
) AS source (IndicatorCode, IndicatorName, Source, Description, DefaultWeight, Category)
ON target.IndicatorCode = source.IndicatorCode
WHEN NOT MATCHED THEN INSERT (IndicatorCode, IndicatorName, Source, Description, DefaultWeight, Category)
VALUES (source.IndicatorCode, source.IndicatorName, source.Source, source.Description, source.DefaultWeight, source.Category);
GO
