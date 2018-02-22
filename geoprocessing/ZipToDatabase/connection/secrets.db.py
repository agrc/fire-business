info = {
    'driver': 'SQL Server',
    'server': 'dns/ip',
    'database': 'name',
    'username': '',
    'password': ''
}


def get_connection_string(info):
    return 'DRIVER={{{}}};SERVER={};DATABASE={};Uid={};Pwd={}'.format(info['driver'], info['server'], info['database'], info['username'], info['password'])
