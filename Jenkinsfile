pipeline {
    agent any
	
	environment {
		gitlab_user_pwd=credentials('gitlab-user-pwd')
        BuildConfiguration="Debug"
	}

    stages {
        stage('Build') {
            steps {
                sh 'dotnet build -c $BuildConfiguration'
            }
        }
		
		stage('Push packages') {
			steps {
				sh 'dotnet pack -c $BuildConfiguration "Tgs.Payment.Client/Tgs.Payment.Client.csproj" --no-build'
				sh 'dotnet pack -c $BuildConfiguration "Payment.Core/Payment.Core.csproj" --no-build'
				sh 'dotnet nuget push "Tgs.Payment.Client/bin/$BuildConfiguration/*.nupkg" --source "raypro" --skip-duplicate'
				sh 'dotnet nuget push "Payment.Core/bin/$BuildConfiguration/*.nupkg" --source "raypro" --skip-duplicate'
			}
		}
	}
}
