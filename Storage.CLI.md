# The Storage CLI

Example: 
```
docker run -it --name storage -v D:/logs:/logfolder -v D:/files:/filesfolder -p 5000:80 -e log=/logfolder -e storagedir=/filesfolder -e defaultphotopath=default/file.png -e db="Data Source=SQL6001.site4now.net;Initial Catalog=DB_A2C8AC_magnis;User Id=DB_A2C8AC_magnis_admin;Password=Newpassword123;" registry.git.magnise.com/magnis.customers/Fintatech.TDS.Common.storage.api:test
```    

| Parameter | Description | Default |   
| :-----------  | :------------------ | :----- | 
| `ip` | IP | `*` |  
| `port` | Port | `5000` |  
| `tls` | Transport Layer Security | `false` |
| `tlsport` | Transport Layer Security Port | `5443` |  
| `certPath` | Path for certificate | - |
| `certSec` | Sertificate secret | -  |
| `auth` | Authority | -  |
| `log`              | Absolute path for logging directory | - |   
| `db`              | Database connection string | `Data Source=.\\SQLEXPRESS;Initial Catalog=Common.Plurk;Integrated Security=true;` |   
| `profileapi`              | Uri for Profile API | `http://localhost:88/` |   
| `storagedir`              | Directory for saving files | `/files` |   
| `defaultphotopath`              | Relative path for saving default photo | `Default/defaultPhoto.png` |   
