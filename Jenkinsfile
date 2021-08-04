pipeline {
    agent any
	
	environment {
		gitlab_user_pwd=credentials('gitlab-user-pwd')
        BuildConfiguration='Debug'
		ImageName="registry.gitlab.com/rayprosoft/turnkey-gaming-system/payment-api"
	}

    stages {
		stage('Prepare') {
            steps {
                sh "echo 'Branch: $GIT_BRANCH'";

                script {
                    switch(env.GIT_BRANCH)
                    {
                        case 'origin/main':
                            env.kube_ns = "prod";
                            env.k8s_config = 'stag-k8s-config';
                            break;
                        case 'origin/release':
                            env.kube_ns = "staging";
                            env.k8s_config = 'stag-k8s-config';
                            break;
                        default:
                            env.kube_ns = "dev";
                            env.k8s_config='k8s-kube-config' 
                            break;
                    }
                }

                configFileProvider([
					configFile(
						fileId: 'tgs.' + env.kube_ns , 
						variable: 'tgsconfig')]) {
					script {
						def envItems = readProperties file: "$tgsconfig";

                        envItems.each { key, value -> 
                            env."tgs_${key}" = value;
                        }

                        env.TAG = env.kube_ns + '-' + sh(script: "echo `date +%Y%m%d%H%M`", returnStdout: true).trim();
					}
				}

                sh "echo tag: $TAG"
            }
        }

        stage('Build') {
            steps {
                sh 'dotnet build -c $BuildConfiguration'
            }
        }

		stage('Test') {
			steps {
				sh 'dotnet test -c $BuildConfiguration --logger trx --no-build'
			}
		}
		
		stage('Push packages') {
			steps {
				sh 'dotnet pack -c $BuildConfiguration "Tgs.Payment.Client/Tgs.Payment.Client.csproj" --no-build'
				sh 'dotnet nuget push "Tgs.Payment.Client/bin/$BuildConfiguration/*.nupkg" --source "raypro" --skip-duplicate'

				sh 'dotnet pack -c $BuildConfiguration "PaymentService.Client/PaymentService.Client.csproj" --no-build'
				sh 'dotnet nuget push "PaymentService.Client/bin/$BuildConfiguration/*.nupkg" --source "raypro" --skip-duplicate'
			}
		}

		stage('Build Docker Image') {
			steps {
				sh 'dotnet build "PaymentService/PaymentService.csproj" -c $BuildConfiguration -t:ContainerBuild -p:DockerfileTag="$ImageName:v${BUILD_NUMBER}" --no-restore '
			}
		}

		stage('Push Docker Image') {
			steps {
				sh 'sudo docker push $ImageName:v${BUILD_NUMBER}'
			}
		}

		stage('Deploy') {
            when {
                expression {
                    return env.kube_ns != 'dev';
                }
            }

            steps {
                script {
                    input message: 'Ready to go?', ok: 'yes';
                }
                
                sh "echo start deploy"
            }
        }

		stage('Deploy to dev') {
			steps {
				script {
					def json = readJSON file: "$paymentdev";

					env.TGS = json.tgs;
					env.NatsUri = json.nats;

					withKubeConfig([credentialsId: env.k8s_config]) {
						sh '''
							helm template deployment/k8s/payment-api \
								--set image.tag=${TAG} \
								--set namespace=${kube_ns} \
								--set \\"connectionStrings.tgs=\\${tgs_tgs}\\" \
								--set \\"connectionStrings.nats=\\${tgs_nats}\\" 
						'''
						sh '''
							helm template deployment/k8s/payment-api \
								--set image.tag=${TAG} \
								--set namespace=${kube_ns} \
								--set \\"connectionStrings.tgs=\\${tgs_tgs}\\" \
								--set \\"connectionStrings.nats=\\${tgs_nats}\\" | kubectl apply -f -
						'''
					}
				}
			}
		}
	}

	post {
		always {
			step([$class: 'MSTestPublisher', testResultsFile:"**/*.trx", failOnError: false, keepLongStdio: true])
		}
	}
}
