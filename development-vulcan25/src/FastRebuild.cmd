:: Fast Rebuild - Testing Only
call d src
cd Warehouse
msbuild

call d bin
call d exec Warehouse\Driver\WarehouseDriver\WarehouseDriver.dtsx

call d src
cd Staging
msbuild

call d bin
call d exec Staging\Driver\StagingDriver\StagingDriver.dtsx

call d src
cd FakeDataSource
msbuild

call d bin
call d exec FakeDataSource\Driver\FakeDataSourceDriver\FakeDataSourceDriver.dtsx

call d src
cd Stg_FakeDataSource
cd Tables
msbuild

call d bin
call d exec Stg_FakeDataSource\Driver\Stg_FakeDataSourceDriver\Stg_FakeDataSourceDriver.dtsx

call d \