@Library('github-release-helpers@v0.2.1')

pipeline {
	agent { docker "mcr.microsoft.com/dotnet/core/sdk:2.2" }

	options {
		buildDiscarder(logRotator(numToKeepStr:'5'))
		timestamps()
	}

	environment {
		PROJECT_NAME = "PTrampert.Webpack.CacheBuster"
	}

	stages {
		stage('Build Release Info') {
      steps {
        script {
          releaseInfo = generateGithubReleaseInfo(
            'PaulTrampert',
            "$PROJECT_NAME",
            'v',
            'Github User/Pass',
            'https://api.github.com',
            branch,
            env.BUILD_NUMBER
          )

          echo "Next version is ${releaseInfo.nextVersion().toString()}."
          echo "Changelog:\n${releaseInfo.changelogToMarkdown()}"
        }
      }
    }

		stage('Test') {
			steps {
				sh "dotnet test ${PROJECT_NAME}.Test/${PROJECT_NAME}.Test.csproj -l trx"
			}

			post {
				always {
					xunit thresholds: [failed(unstableThreshold: '0')], tools: [MSTest(deleteOutputFiles: true, failIfNotNew: true, skipNoTestFiles: false, stopProcessingIfError: false)]
				}
			}
		}

		stage('Package') {
			steps {
				sh "dotnet pack ${PROJECT_NAME}/${PROJECT_NAME}.csproj -c Release /p:Version=${releaseInfo.nextVersion().toString()}"
			}
		}

		stage('Publish Pre-Release') {
      when { expression{env.BRANCH_NAME != 'master'} }
      environment {
        API_KEY = credentials('nexus-nuget-apikey')
      }
      steps {
        sh "dotnet nuget push **/*.nupkg -s 'https://packages.ptrampert.com/repository/nuget-prereleases/' -k ${env.API_KEY}"
      }
    }

		stage('Publish Release') {
      when { expression {env.BRANCH_NAME == 'master'} }
      environment {
        API_KEY = credentials('nuget-api-key')
      }
      steps {
        sh "dotnet nuget push **/*.nupkg -s https://api.nuget.org/v3/index.json -k ${env.API_KEY}"

				script {
          publishGithubRelease(
            'PaulTrampert',
            PROJECT_NAME,
            releaseInfo,
            'v',
            'Github User/Pass',
            'https://api.github.com'
          )
        }
      }
    }
	}

	post {
		changed {
			mail(
        to: 'paul.trampert@ptrampert.com',
        subject: "Build status of ${env.JOB_NAME} changed to ${currentBuild.result}", body: "Build log may be found at ${env.BUILD_URL}"
      )
		}
	}
}