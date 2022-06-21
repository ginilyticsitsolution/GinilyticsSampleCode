-- phpMyAdmin SQL Dump
-- version 5.1.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Generation Time: Feb 10, 2022 at 01:42 PM
-- Server version: 10.4.21-MariaDB
-- PHP Version: 8.0.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `ci_test`
--

-- --------------------------------------------------------

--
-- Table structure for table `products`
--

CREATE TABLE `products` (
  `id` int(11) NOT NULL,
  `title` varchar(255) NOT NULL,
  `description` varchar(255) NOT NULL,
  `image` varchar(255) NOT NULL,
  `status` int(11) NOT NULL,
  `created_at` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `products`
--

INSERT INTO `products` (`id`, `title`, `description`, `image`, `status`, `created_at`) VALUES
(1, 'Mobile', 'Mobile', 'mobile.jpg', 1, '2022-02-08 09:31:53'),
(2, 'Keyboard', 'Keyboard', 'mobile.jpg', 2, '2022-02-08 13:28:18'),
(3, 'DSLR Camera', 'DSLR Camera', 'mobile.jpg', 1, '2022-02-08 12:55:56');

-- --------------------------------------------------------

--
-- Table structure for table `users`
--

CREATE TABLE `users` (
  `id` int(11) NOT NULL,
  `name` varchar(255) NOT NULL,
  `email` varchar(255) NOT NULL,
  `password` varchar(255) NOT NULL,
  `role` int(11) NOT NULL,
  `created_at` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `is_email_verified` tinyint(1) NOT NULL,
  `status` enum('active','inactive') NOT NULL DEFAULT 'inactive'
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `users`
--

INSERT INTO `users` (`id`, `name`, `email`, `password`, `role`, `created_at`, `is_email_verified`, `status`) VALUES
(1, 'Nand', 'nand1235@yopmail.com', '12345678', 1, '2022-02-08 11:57:32', 1, 'active'),
(2, 'Nand', 'nand@gmail.com', '12345678', 1, '2022-02-08 11:57:33', 1, 'active'),
(3, 'admin', 'admin@gmail.com', '1234', 1, '2022-02-08 11:57:35', 1, 'active'),
(4, 'test', 'nand@ginilytics.com', '123', 1, '2022-02-08 09:02:23', 0, 'inactive'),
(5, 'test', 'test@test.com', '111', 1, '2022-02-08 09:04:13', 0, 'inactive'),
(6, 'ttest', 'test@tge.com', '123', 1, '2022-02-08 11:57:37', 1, 'active'),
(7, '11', 'superadmin@gmail.com', '12', 1, '2022-02-08 09:11:20', 0, 'inactive'),
(8, 'Test', 'test@gmail.com', '1234', 1, '2022-02-09 09:00:00', 0, 'inactive'),
(9, 'Nand', 'nandmaithani178@yopmail.com', '123', 1, '2022-02-10 10:38:24', 0, 'inactive'),
(10, 'Nand', 'nandmaithani177@yopmail.com', '123', 1, '2022-02-10 10:50:48', 0, 'inactive'),
(11, 'Nand', 'nandmaithani176@yopmail.com', '123', 1, '2022-02-10 11:28:17', 1, 'inactive'),
(12, 'admin', 'admin901@yopmail.com', '1234', 1, '2022-02-10 11:30:58', 0, 'inactive'),
(13, 'nand', 'admin902@yopmail.com', '1234', 1, '2022-02-10 12:40:06', 0, 'inactive');

-- --------------------------------------------------------

--
-- Table structure for table `user_products`
--

CREATE TABLE `user_products` (
  `id` int(11) NOT NULL,
  `product_id` int(11) NOT NULL,
  `user_id` int(11) NOT NULL,
  `quantity` int(11) NOT NULL,
  `price` varchar(255) NOT NULL,
  `created_at` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `user_products`
--

INSERT INTO `user_products` (`id`, `product_id`, `user_id`, `quantity`, `price`, `created_at`) VALUES
(1, 1, 1, 3, '100', '2022-02-08 13:30:39'),
(2, 2, 1, 7, '200', '2022-02-08 13:30:38'),
(3, 2, 1, 20, '300', '2022-02-08 13:30:35'),
(4, 2, 1, 30, '5550', '2022-02-08 13:30:32'),
(5, 1, 1, 5, '400', '2022-02-08 13:30:29'),
(6, 2, 1, 5, '500', '2022-02-08 13:30:27'),
(7, 1, 1, 40, '500', '2022-02-08 13:30:24'),
(8, 1, 1, 10, '3434', '2022-02-08 13:30:22'),
(10, 1, 3, 6, '200', '2022-02-09 05:01:01'),
(11, 1, 3, 7, '200', '2022-02-09 05:13:24'),
(12, 2, 3, 8, '300', '2022-02-09 05:13:34'),
(13, 3, 3, 9, '300', '2022-02-09 05:13:46'),
(14, 3, 3, 8, '300', '2022-02-09 05:40:24'),
(15, 2, 3, 8, '400', '2022-02-09 05:40:40'),
(16, 3, 3, 6, '200', '2022-02-09 07:46:49');

--
-- Indexes for dumped tables
--

--
-- Indexes for table `products`
--
ALTER TABLE `products`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `user_products`
--
ALTER TABLE `user_products`
  ADD PRIMARY KEY (`id`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `products`
--
ALTER TABLE `products`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT for table `users`
--
ALTER TABLE `users`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=14;

--
-- AUTO_INCREMENT for table `user_products`
--
ALTER TABLE `user_products`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=17;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
