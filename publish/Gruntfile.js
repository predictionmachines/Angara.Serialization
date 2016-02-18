/// <binding />
module.exports = function (grunt) {

    var bower = grunt.file.readJSON("../bower.json");
    grunt.log.write("=== Publishing version: " + bower.version + " ===")

    grunt.initConfig({     
        version: bower.version,
        clean: ["./*.nupkg" ],
        nugetrestore: {
            all: {
                src: ['../src/Angara.Serialization.Json/packages.config'],
                dest: ['../ext/nuget']
            }
        },
        nugetPackFS: {
            "serialization": "../src/Angara.Serialization/Angara.Serialization.fsproj",
            "serialization.json": "../src/Angara.Serialization.Json/Angara.Serialization.Json.fsproj",
        },
        nugetpush: {
            all: {
               src: '*.nupkg' 
            }
        },
        gitcheckout: {
            release: {
                options: {
                    branch: 'r<%=version%>',
                    create: true
                }
            },
            master: {
                options: {
                    branch: 'master'
                }
            }

        },
        gitadd: {
            js: {
                options: {
                    force: true
                },
                files: {
                    src: ['../dist/*']
                }
            }
        },
        gitcommit: {
            js: {
                option: {
                    message: "Release <%=version%>"
                },
                files: {
                    src: ['../dist/*']
                }
            }
        },
        gitdbranch: {
            release: 'r<%=version%>'
        },
        gittag: {
            release: {
               options: {
                   tag: "v<%=version%>"
               } 
            }  
        },
        gitpush: {
            release: {
                options: {
                    tags: true
                }
            }
        }
    });

    grunt.loadNpmTasks('grunt-nuget');
    grunt.loadNpmTasks('grunt-contrib-clean');
    grunt.loadNpmTasks('grunt-git');
        
    // Bug in current grunt-nuget doesn't allow to build .fsproj
    grunt.registerMultiTask('nugetPackFS', 'Create nuget package from .fsproj', function() {
       grunt.log.writeln("Creating package from " + this.data); 
       var executable = "node_modules/grunt-nuget/libs/nuget.exe"; // I assume that grunt-nuget 0.1.5 is installed
       var done = this.async();
       //invoke nuget.exe
       grunt.util.spawn({
           cmd: executable,
           args: [
               //specify the .nuspec file
               "pack",
               this.data,
 
               //specify where we want the package to be created
               "-OutputDirectory",
               ".",
 
               "-Build",
               "-IncludeReferencedProjects",
               "-Prop",
               "Configuration=Release"
           ] 
        }, function (error, result) {
            //output either result text or error message...
            if (error) {
                grunt.log.error(error);
            } else {
                grunt.log.write(result);
            }
            done();
        });
    });
    
    // grunt-git doesn't support branch 
    grunt.registerMultiTask('gitdbranch', 'Delete Git branch', function() {
       grunt.log.writeln("Deleting git branch " + this.data); 
       var executable = "git.exe"; // Should be in path
       var done = this.async();
       //invoke nuget.exe
       grunt.util.spawn({
           cmd: executable,
           args: [
               "branch",
               "-D",
               this.data
           ] 
        }, function (error, result) {
            //output either result text or error message...
            if (error) {
                grunt.log.error(error);
            } else {
                grunt.log.write(result);
            }
            done();
        });
    });
    
    grunt.registerTask('make-bower', ['gitcheckout:release','gitadd','gitcommit','gittag','gitcheckout:master','gitdbranch'])
    grunt.registerTask('make-nuget', ['clean','nugetrestore', 'nugetPackFS'])
    grunt.registerTask('default', ['run-main-grunt', 'make-bower', 'make-nuget', 'gitpush', 'nugetpush']);
    grunt.registerTask('nopush', ['run-main-grunt',  'make-bower', 'make-nuget']);
    
    grunt.registerTask('run-main-grunt', function () {
        var done = this.async();
        grunt.util.spawn({
            cmd: 'grunt',
            opts: {
                cwd: '..', // Run main Gruntfile.js
                stdio: 'inherit' 
            }
        }, function (err, result, code) {
            //output either result text or error message...
            if (err) {
                grunt.fail.fatal(err);
            } else {
                grunt.log.write(result);
            }
            done();
        });
    });
};