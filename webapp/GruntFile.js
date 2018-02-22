module.exports = function configure(grunt) {
    require('load-grunt-tasks')(grunt);

    var jsAppFiles = '_src/app/**/*.js';
    var otherFiles = [
        '_src/app/**/*.html',
        '_src/app/**/*.styl',
        '_src/index.html',
        '_src/ChangeLog.html'
    ];
    var gruntFile = 'GruntFile.js';
    var internFile = 'tests/intern.js';
    var jsFiles = [
        jsAppFiles,
        gruntFile,
        internFile,
        'profiles/**/*.js'
    ];
    var bumpFiles = [
        'package.json',
        'bower.json',
        '_src/app/package.json',
        '_src/app/config.js'
    ];

    // Project configuration.
    grunt.initConfig({
        pkg: grunt.file.readJSON('package.json'),
        amdcheck: {
            main: {
                options: {
                    removeUnusedDependencies: false
                },
                files: [{
                    src: [
                        'src/app/**/*.js'
                    ]
                }]
            }
        },
        babel: {
            options: {
                sourceMap: false,
                presets: ['es2015-without-strict']
            },
            src: {
                files: [{
                    expand: true,
                    cwd: '_src',
                    src: ['**/*.js'],
                    dest: 'src'
                }]
            }
        },
        bump: {
            options: {
                files: bumpFiles,
                commitFiles: bumpFiles,
                push: false
            }
        },
        clean: {
            src: ['src/app'],
            build: ['dist'],
            deploy: ['deploy']
        },
        connect: {
            uses_defaults: { // eslint-disable-line camelcase
            }
        },
        copy: {
            src: {
                expand: true,
                cwd: '_src',
                src: ['**/*.html', '**/*.css', '**/*.png', '**/*.jpg', 'secrets.json', 'app/packages.json'],
                dest: 'src'
            }
        },
        dojo: {
            local: {
                options: {
                    profiles: ['profiles/build.profile.js'],
                    releaseDir: './dist'
                }
            },
            stage: {
                options: {
                    // You can also specify options to be used in all your tasks
                    profiles: ['profiles/stage.build.profile.js', 'profiles/build.profile.js']
                }
            },
            prod: {
                options: {
                    // You can also specify options to be used in all your tasks
                    profiles: ['profiles/prod.build.profile.js', 'profiles/build.profile.js']
                }
            },
            options: {
                // You can also specify options to be used in all your tasks
                dojo: 'src/dojo/dojo.js',
                load: 'build',
                releaseDir: '../dist',
                requires: ['src/app/packages.js', 'src/app/run.js'],
                basePath: './src'
            }
        },
        eslint: {
            options: {
                configFile: '.eslintrc'
            },
            main: {
                src: jsFiles
            }
        },
        imagemin: {
            main: {
                options: {
                    optimizationLevel: 3
                },
                files: [{
                    expand: true,
                    cwd: '_src/',
                    // exclude tests because some images in dojox throw errors
                    src: ['**/*.{png,jpg,gif}', '!**/tests/**/*.*'],
                    dest: '_src/'
                }]
            }
        },
        jasmine: {
            main: {
                options: {
                    specs: ['src/app/**/Spec*.js'],
                    vendor: [
                        'src/app/tests/jasmineTestBootstrap.js',
                        'src/dojo/dojo.js',
                        'src/app/packages.js'
                    ],
                    host: 'http://localhost:8000',
                    keepRunner: true
                }
            }
        },
        parallel: {
            options: {
                grunt: true
            },
            assets: {
                tasks: ['eslint', 'amdcheck', 'stylus', 'babel', 'copy:src', 'jasmine:main:build']
            },
            buildAssets: {
                tasks: ['eslint', 'clean:build', 'stylus', 'babel', 'copy:src']
            }
        },
        processhtml: {
            options: {},
            main: {
                files: {
                    'dist/index.html': ['src/index.html'],
                    'dist/user_admin.html': ['src/user_admin.html']
                }
            }
        },
        stylint: {
            src: ['_src/**/*.styl']
        },
        stylus: {
            main: {
                options: {
                    compress: false,
                    'resolve url': true
                },
                files: [{
                    expand: true,
                    cwd: '_src/',
                    src: ['app/**/*.styl'],
                    dest: 'src/',
                    ext: '.css'
                }]
            }
        },
        uglify: {
            options: {
                preserveComments: false,
                sourceMap: true,
                compress: {
                    drop_console: true, // eslint-disable-line camelcase
                    passes: 2,
                    dead_code: true // eslint-disable-line camelcase
                }
            },
            stage: {
                options: {
                    compress: {
                        drop_console: false // eslint-disable-line camelcase
                    }
                },
                src: ['dist/dojo/dojo.js'],
                dest: 'dist/dojo/dojo.js'
            },
            prod: {
                files: [{
                    expand: true,
                    cwd: 'dist',
                    src: '**/*.js',
                    dest: 'dist'
                }]
            }
        },
        watch: {
            src: {
                files: jsFiles.concat(otherFiles),
                options: { livereload: true },
                tasks: ['eslint', 'amdcheck', 'stylint', 'stylus', 'babel',
                    'copy:src', 'jasmine:main:build']
            }
        }
    });

    grunt.registerTask('default', [
        'clean:src',
        'parallel:assets',
        'copy:src',
        'connect',
        'watch'
    ]);
    grunt.registerTask('build', [
        'clean:src',
        'clean:build',
        'parallel:buildAssets',
        'newer:imagemin:main',
        'dojo:local',
        'processhtml:main'
    ]);
    grunt.registerTask('build-stage', [
        'clean:src',
        'clean:build',
        'parallel:buildAssets',
        'newer:imagemin:main',
        'dojo:stage',
        'uglify:stage',
        'processhtml:main'
    ]);
    grunt.registerTask('build-prod', [
        'clean:src',
        'clean:build',
        'parallel:buildAssets',
        'newer:imagemin:main',
        'dojo:prod',
        'uglify:prod',
        'processhtml:main'
    ]);
    grunt.registerTask('test', [
        'clean:src',
        'parallel:assets',
        'copy:src',
        'connect',
        'jasmine'
    ]);
    grunt.registerTask('travis', [
        'eslint',
        'test',
        'build-prod'
    ]);
};
