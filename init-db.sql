-- 初始化数据库脚本
-- 创建数据库（如果不存在）
SELECT 'CREATE DATABASE LocationShareDB'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'LocationShareDB');

-- 连接到LocationShareDB数据库
\c LocationShareDB;

-- 创建扩展（如果需要）
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 设置时区
SET timezone = 'UTC';

-- 创建索引优化查询性能
-- 这些索引将在Entity Framework迁移后创建，这里仅作为参考

-- 用户表索引
-- CREATE INDEX IF NOT EXISTS idx_users_phone_number ON "Users" ("PhoneNumber");
-- CREATE INDEX IF NOT EXISTS idx_users_binding_code ON "Users" ("BindingCode");
-- CREATE INDEX IF NOT EXISTS idx_users_last_active ON "Users" ("LastActiveAt");

-- 位置表索引
-- CREATE INDEX IF NOT EXISTS idx_user_locations_user_timestamp ON "UserLocations" ("UserId", "Timestamp" DESC);
-- CREATE INDEX IF NOT EXISTS idx_user_locations_timestamp ON "UserLocations" ("Timestamp" DESC);

-- 电量表索引
-- CREATE INDEX IF NOT EXISTS idx_user_batteries_user_timestamp ON "UserBatteries" ("UserId", "Timestamp" DESC);
-- CREATE INDEX IF NOT EXISTS idx_user_batteries_timestamp ON "UserBatteries" ("Timestamp" DESC);

-- 用户关联表索引
-- CREATE INDEX IF NOT EXISTS idx_user_connections_user ON "UserConnections" ("UserId");
-- CREATE INDEX IF NOT EXISTS idx_user_connections_connected_user ON "UserConnections" ("ConnectedUserId");

-- 设置数据库参数优化性能
ALTER SYSTEM SET shared_preload_libraries = 'pg_stat_statements';
ALTER SYSTEM SET max_connections = 200;
ALTER SYSTEM SET shared_buffers = '256MB';
ALTER SYSTEM SET effective_cache_size = '1GB';
ALTER SYSTEM SET maintenance_work_mem = '64MB';
ALTER SYSTEM SET checkpoint_completion_target = 0.9;
ALTER SYSTEM SET wal_buffers = '16MB';
ALTER SYSTEM SET default_statistics_target = 100;

-- 重新加载配置
SELECT pg_reload_conf();

COMMIT;