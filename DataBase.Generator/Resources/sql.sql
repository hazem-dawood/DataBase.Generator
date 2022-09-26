SELECT sc.name as SchemaName, T.name AS TableName ,
    C.name AS ColumnName ,
    P.name AS DataType ,0 IsUpdate,0 IsDelete,0 IsInsert,0 IsAfter,0 IsInsteadOf,0 [Disabled],
	'Table'[Type],c.is_nullable IsNullable,object_definition(T.object_id) Defainition,''[ReferencedTable],''[ReferencedColumn],
	cast(iif(exists(SELECT top 1 COLUMN_NAME
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + QUOTENAME(CONSTRAINT_NAME)), 'IsPrimaryKey') = 1
AND TABLE_NAME = T.name AND TABLE_SCHEMA = sc.name and COLUMN_NAME = c.name),1,0) as bit)PrimaryKey
FROM   sys.objects AS T																	
    JOIN sys.columns AS C ON T.object_id = C.object_id
    JOIN sys.types AS P ON C.system_type_id = P.system_type_id
	inner join sys.Schemas sc on sc.schema_id = t.schema_id
WHERE  T.type_desc = 'USER_TABLE'
union
SELECT 
SCHEMA_NAME(t.schema_id),
     TableName = t.name,
     IndexName = ind.name,
     ColumnName = col.name
	 ,0 IsUpdate,0 IsDelete,0 IsInsert,0 IsAfter,0 IsInsteadOf,0 [Disabled],
	'Index'[Type],0,object_definition(ind.object_id) Defainition,'','',0
FROM 
     sys.indexes ind 
INNER JOIN 
     sys.index_columns ic ON  ind.object_id = ic.object_id and ind.index_id = ic.index_id 
INNER JOIN 
     sys.columns col ON ic.object_id = col.object_id and ic.column_id = col.column_id 
INNER JOIN 
     sys.tables t ON ind.object_id = t.object_id 
WHERE 
     ind.is_primary_key = 0 
     AND ind.is_unique = 0 
     AND ind.is_unique_constraint = 0 
     AND t.is_ms_shipped = 0 

union
SELECT s.name AS table_schema ,OBJECT_NAME(parent_obj) AS table_name , sysobjects.name AS trigger_name ,''
,OBJECTPROPERTY( id, 'ExecIsUpdateTrigger') AS isupdate ,OBJECTPROPERTY( id, 'ExecIsDeleteTrigger') AS isdelete 
,OBJECTPROPERTY( id, 'ExecIsInsertTrigger') AS isinsert ,OBJECTPROPERTY( id, 'ExecIsAfterTrigger') AS isafter 
,OBJECTPROPERTY( id, 'ExecIsInsteadOfTrigger') AS isinsteadof ,OBJECTPROPERTY(id, 'ExecIsTriggerDisabled') AS [disabled] ,
'Trigger',0,object_definition(object_id) Defainition,'','',0
FROM sysobjects INNER JOIN sys.tables t ON sysobjects.parent_obj = t.object_id 
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id  WHERE sysobjects.type = 'TR' 
union
SELECT SCHEMA_NAME(schema_id) AS [Schema],[name],'','', 0 IsUpdate,0 IsDelete,0 IsInsert,0 IsAfter,0 IsInsteadOf,0 [Disabled],'Stored Procedure'[Type],0,
object_definition(object_id) Defainition,'','',0
FROM sys.objects WHERE type = 'P' 
union

SELECT sc.name, v.name  AS View_Name
      ,c.name  AS Column_Name,p.name, 0 IsUpdate,0 IsDelete,0 IsInsert,0 IsAfter,0 IsInsteadOf,0 [Disabled],'View'[Type],c.is_nullable,
	  object_definition(v.object_id),'','',0
FROM sys.views  v 
INNER JOIN sys.all_columns  c  ON v.object_id = c.object_id
inner join sys.Schemas sc on sc.schema_id = v.schema_id
JOIN sys.types AS P ON C.system_type_id = P.system_type_id
union

SELECT sch.name AS [schema_name],tab1.name AS [table],
col1.name AS [column],
 P1.name AS DataType ,0 IsUpdate,0 IsDelete,0 IsInsert,0 IsAfter,0 IsInsteadOf,0 [Disabled],'Constrain',0, obj.name AS FK_NAME,
tab2.name AS [ReferencedTable],
col2.name AS [ReferencedColumn],0
FROM sys.foreign_key_columns fkc
INNER JOIN sys.objects obj ON obj.object_id = fkc.constraint_object_id
INNER JOIN sys.tables tab1 ON tab1.object_id = fkc.parent_object_id
INNER JOIN sys.schemas sch ON tab1.schema_id = sch.schema_id
INNER JOIN sys.columns col1 ON col1.column_id = parent_column_id AND col1.object_id = tab1.object_id
JOIN sys.types AS P1 ON col1.system_type_id = P1.system_type_id
INNER JOIN sys.tables tab2 ON tab2.object_id = fkc.referenced_object_id
INNER JOIN sys.columns col2 ON col2.column_id = referenced_column_id AND col2.object_id = tab2.object_id