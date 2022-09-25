SELECT sc.name as SchemaName, T.name AS TableName ,
    C.name AS ColumnName ,
    P.name AS DataType ,0 IsUpdate,0 IsDelete,0 IsInsert,0 IsAfter,0 IsInsteadOf,0 [Disabled],
	'Table'[Type],c.is_nullable IsNullable
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
	'Index'[Type],0
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
'Trigger',0
FROM sysobjects INNER JOIN sys.tables t ON sysobjects.parent_obj = t.object_id 
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id  WHERE sysobjects.type = 'TR' 
union
SELECT SCHEMA_NAME(schema_id) AS [Schema],[name],'','', 0 IsUpdate,0 IsDelete,0 IsInsert,0 IsAfter,0 IsInsteadOf,0 [Disabled],'Stored Procedure'[Type],0
FROM sys.objects WHERE type = 'P' 
union
SELECT sc.name, v.name  AS View_Name
      ,c.name  AS Column_Name,p.name, 0 IsUpdate,0 IsDelete,0 IsInsert,0 IsAfter,0 IsInsteadOf,0 [Disabled],'View'[Type],c.is_nullable
FROM sys.views  v 
INNER JOIN sys.all_columns  c  ON v.object_id = c.object_id
inner join sys.Schemas sc on sc.schema_id = v.schema_id
JOIN sys.types AS P ON C.system_type_id = P.system_type_id
union
select [Schema], table_view,
constraint_name,
    details + ' : ' + constraint_type
	, 0 IsUpdate,0 IsDelete,0 IsInsert,0 IsAfter,0 IsInsteadOf,0 [Disabled],'Constrain'[Type],0
    
from (
    select schema_name(t.schema_id)[Schema] , t.[name] as table_view, 
        case when c.[type] = 'PK' then 'Primary key'
            when c.[type] = 'UQ' then 'Unique constraint'
            when i.[type] = 1 then 'Unique clustered index'
            when i.type = 2 then 'Unique index'
            end as constraint_type, 
        isnull(c.[name], i.[name]) as constraint_name,
        substring(column_names, 1, len(column_names)-1) as [details]
    from sys.objects t
        left outer join sys.indexes i
            on t.object_id = i.object_id
        left outer join sys.key_constraints c
            on i.object_id = c.parent_object_id 
            and i.index_id = c.unique_index_id
       cross apply (select col.[name] + ', '
                        from sys.index_columns ic
                            inner join sys.columns col
                                on ic.object_id = col.object_id
                                and ic.column_id = col.column_id
                        where ic.object_id = t.object_id
                            and ic.index_id = i.index_id
                                order by col.column_id
                                for xml path ('') ) D (column_names)
    where is_unique = 1
    and t.is_ms_shipped <> 1
    union all 
    select schema_name(fk_tab.schema_id) + '.' + fk_tab.name as foreign_table,
        'Table',
        'Foreign key',
        fk.name as fk_constraint_name,
        schema_name(pk_tab.schema_id) + '.' + pk_tab.name
    from sys.foreign_keys fk
        inner join sys.tables fk_tab
            on fk_tab.object_id = fk.parent_object_id
        inner join sys.tables pk_tab
            on pk_tab.object_id = fk.referenced_object_id
        inner join sys.foreign_key_columns fk_cols
            on fk_cols.constraint_object_id = fk.object_id
    union all
    select schema_name(t.schema_id) + '.' + t.[name],
        'Table',
        'Check constraint',
        con.[name] as constraint_name,
        con.[definition]
    from sys.check_constraints con
        left outer join sys.objects t
            on con.parent_object_id = t.object_id
        left outer join sys.all_columns col
            on con.parent_column_id = col.column_id
            and con.parent_object_id = col.object_id
    union all
    select schema_name(t.schema_id) + '.' + t.[name],
        'Table',
        'Default constraint',
        con.[name],
        col.[name] + ' = ' + con.[definition]
    from sys.default_constraints con
        left outer join sys.objects t
            on con.parent_object_id = t.object_id
        left outer join sys.all_columns col
            on con.parent_column_id = col.column_id
            and con.parent_object_id = col.object_id) a
