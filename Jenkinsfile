pipeline {
    agent any
	
	environment {
		gitlab_user_pwd=credentials('gitlab-user-pwd')
        BuildConfiguration='Debug'
		ImageName="registry.gitlab.com/rayprosoft/turnkey-gaming-system/payment-api"
	}

    stages {
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

		stage('Deploy to dev') {
			input {
				message 'Ready to go?'
				ok 'yes'
			}

			steps {
				configFileProvider([
					configFile(
						fileId: 'payment.dev.json', 
						variable: 'paymentdev')]) {
					script {
						def json = readJSON file: "$paymentdev";

						env.TGS = json.tgs;
						env.NatsUri = json.nats;

						withKubeConfig([credentialsId: 'k8s-kube-config', serverUrl: 'https://192.168.50.105:6443']) {
							sh '''
								helm template k8s/payment \
									--set image.tag=v${BUILD_NUMBER} \
									--set namespace=dev \
									--set "connectionStrings.tgs=${TGS}" \
									--set nats=${NatsUri} 
							'''
							sh '''
								helm template k8s/payment \
									--set image.tag=v${BUILD_NUMBER} \
									--set namespace=dev \
									--set "connectionStrings.tgs=${TGS}" \
									--set nats=${NatsUri} | kubectl -n dev apply -f -
							'''
						}
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
