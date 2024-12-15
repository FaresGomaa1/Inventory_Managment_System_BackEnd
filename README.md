# **Product Management Backend System**

### **Overview**  
This is the backend repository for a product management system. The backend is built with **ASP.NET Core** and provides essential APIs for managing products, users, and authentication. It connects seamlessly with the front-end system and database to ensure efficient data handling and secure operations.

---

## **Features**  
- **Product Management**  
   - Create, Read, Update, and Delete (CRUD) operations for product details.  
   - Fetch product data dynamically using RESTful APIs.  

- **User Authentication and Authorization**  
   - Implements **Identity Framework** for secure user registration, login, and role-based access.  

- **Database Integration**  
   - Connects with **MS SQL Server** to store product data, user information, and transactional details.  

- **Email Notifications**  
   - Integrates with **EmailJs** for sending notifications such as order updates and user confirmations.  

- **Web API Development**  
   - Provides clean and structured endpoints for front-end communication.  

---

## **Technologies Used**  
| Technology         | Purpose                    |  
|--------------------|----------------------------|  
| **ASP.NET Core**   | Backend framework          |  
| **MS SQL Server**  | Database management        |  
| **Identity Framework** | User authentication and authorization |  
| **Web API**        | RESTful API for front-end communication |  
| **EmailJs**        | Third-party email service  |  

---

## **Setup Instructions**

Follow these steps to set up and run the backend on your local machine:

### **1. Prerequisites**  
- Install [.NET SDK](https://dotnet.microsoft.com/download) (latest version).  
- Install **MS SQL Server** and set up a database.  
- (Optional) Install a tool like **Postman** for API testing.  

### **2. Clone the Repository**  
```bash
git clone https://github.com/FaresGomaa1/Inventory_Managment_System_BackEnd.git
cd backend-product-management
```

### **3. Configure the Database**  
- Open the `appsettings.json` file.  
- Update the connection string for your **SQL Server**:  
   ```json
   "ConnectionStrings": {
      "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=YOUR_DB_NAME;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;"
   }
   ```

### **4. Run Migrations**  
To create the database schema, run the following commands:  
```bash
dotnet ef database update
```

### **5. Run the Application**  
Start the server:  
```bash
dotnet run
```
The API will be available at: `http://localhost:5000` (default).

---

## **API Endpoints**

Here are the main endpoints for the backend:

| Endpoint              | Method   | Description                       |  
|-----------------------|----------|-----------------------------------|  
| `/api/products`       | GET      | Get all products                  |  
| `/api/products/{id}`  | GET      | Get product by ID                 |  
| `/api/products`       | POST     | Create a new product              |  
| `/api/products/{id}`  | PUT      | Update an existing product        |  
| `/api/products/{id}`  | DELETE   | Delete a product                  |  
| `/api/auth/register`  | POST     | User registration                 |  
| `/api/auth/login`     | POST     | User login and token generation   |  

---

## **Testing**

- Use **Postman** or tools like **Swagger UI** to test the API endpoints.  
- Write unit tests for critical components using **xUnit** or **NUnit**.

---

## **Future Improvements**  
- Add pagination and filtering for product data.  
- Implement role-based authorization for API endpoints.  
- Enhance logging and error handling.

---

## **Contributing**

I welcome contributions!  
1. Fork the repository.  
2. Create a new branch:  
   ```bash
   git checkout -b feature-branch
   ```
3. Make your changes and commit:  
   ```bash
   git commit -m "Add a new feature"
   ```
4. Push to the branch:  
   ```bash
   git push origin feature-branch
   ```
5. Create a Pull Request.

---

## **License**  
This project is licensed under the [MIT License](LICENSE).  

---

## **Contact**  
For any questions or suggestions, feel free to reach out:  
- **Fares Gomaa**: [fares.gomaa.work@gmail.com](fares.gomaa.work@gmail.com)  
- **GitHub**: [FaresGomaa1](https://github.com/FaresGomaa1)

---

### **Acknowledgments**  
- Special thanks to the developers and resources behind ASP.NET Core and other tools.
